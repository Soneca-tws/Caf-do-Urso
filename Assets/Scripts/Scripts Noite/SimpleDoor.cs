using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using TMPro;
using PrimeTween;

public class SimpleDoor : MonoBehaviour
{
    [Header("Configuração de Abertura")]
    [Tooltip("Velocidade mínima para abrir correndo (também oculta o aviso de UI).")]
    [SerializeField] private float minSpeedToBreak = 4.5f;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float openSpeed = 550f;
    [SerializeField] private bool hingeOnRight = false;

    [Header("Interação com Tecla E e UI")]
    [SerializeField] private TextMeshProUGUI textoAvisoUI;
    [SerializeField] private string mensagemAbrir = "Aperte a tecla E para abrir a porta";
    [SerializeField] private float distanciaInteracao = 2.6f;
    [Tooltip("0.5 exige que o jogador esteja olhando na direção da porta (~60°).")]
    [SerializeField] [Range(0f, 1f)] private float precisaoDeFrente = 0.5f;

    [Header("Áudio (Opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private float soundVolume = 1f;

    private static SimpleDoor portaEmFoco;

    private BoxCollider solidCollider;
    private NavMeshObstacle navObstacle;
    private PlayerMovement jogadorPrincipal;
    private Rigidbody jogadorRb;
    private PlayerMovement playerInZone;
    private Rigidbody playerRb;

    private bool isOpen = false;
    private Vector3 hingePoint;
    private Vector3 widthAxis;
    private Vector3 normalAxis;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private Tween tweenAbertura;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
        CalcularDobradica();
    }

    private void Start()
    {
        BuscarReferenciasJogador();

        if (textoAvisoUI != null && portaEmFoco == null)
        {
            textoAvisoUI.gameObject.SetActive(false);
        }
    }

    private void BuscarReferenciasJogador()
    {
        jogadorPrincipal = FindFirstObjectByType<PlayerMovement>();
        if (jogadorPrincipal != null)
        {
            jogadorRb = jogadorPrincipal.GetComponent<Rigidbody>();
        }
    }

    private void CalcularDobradica()
    {
        foreach (BoxCollider col in GetComponents<BoxCollider>())
        {
            if (!col.isTrigger)
            {
                solidCollider = col;
                break;
            }
        }

        float sizeX = solidCollider != null
            ? solidCollider.size.x * Mathf.Abs(transform.lossyScale.x)
            : Mathf.Abs(transform.lossyScale.x);

        float sizeZ = solidCollider != null
            ? solidCollider.size.z * Mathf.Abs(transform.lossyScale.z)
            : Mathf.Abs(transform.lossyScale.z);

        // Detecta automaticamente se a largura da porta está no eixo X ou no eixo Z
        float doorWidth;
        if (sizeZ > sizeX)
        {
            widthAxis = transform.forward;
            normalAxis = transform.right;
            doorWidth = sizeZ;
        }
        else
        {
            widthAxis = transform.right;
            normalAxis = transform.forward;
            doorWidth = sizeX;
        }

        float sideMultiplier = hingeOnRight ? 1f : -1f;
        hingePoint = transform.position + widthAxis * (doorWidth * 0.5f * sideMultiplier);
    }

    private void Update()
    {
        if (isOpen) return;

        if (jogadorPrincipal == null)
        {
            BuscarReferenciasJogador();
        }

        // 1. Verifica se o jogador está perto e virado de frente para esta porta
        if (VerificarJogadorDeFrente())
        {
            if (portaEmFoco == null || portaEmFoco == this)
            {
                portaEmFoco = this;

                // Só exibe o aviso na tela se o jogador NÃO estiver correndo ou dando dash
                MostrarAviso(!JogadorEstaCorrendo());

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Vector3 posOrigem = jogadorPrincipal != null ? jogadorPrincipal.transform.position : Camera.main.transform.position;
                    OpenDoor(posOrigem);
                    return;
                }
            }
        }
        else if (portaEmFoco == this)
        {
            LibertarFoco();
        }

        // 2. Continua checando abertura por corrida/dash dentro da zona de trigger
        if (playerInZone != null)
        {
            CheckSpeedAndOpen(playerInZone, playerRb);
        }
    }

    private bool JogadorEstaCorrendo()
    {
        if (jogadorPrincipal == null) return false;

        bool isDashing = jogadorPrincipal.state == PlayerMovement.MovementState.dashing;
        bool isRunningFast = jogadorRb != null && jogadorRb.linearVelocity.magnitude >= minSpeedToBreak;

        return isDashing || isRunningFast;
    }

    private bool VerificarJogadorDeFrente()
    {
        Transform refPosicao = jogadorPrincipal != null ? jogadorPrincipal.transform : (Camera.main != null ? Camera.main.transform : null);
        Transform refVisao = Camera.main != null ? Camera.main.transform : refPosicao;

        if (refPosicao == null || refVisao == null) return false;

        Vector3 centroPorta = solidCollider != null ? solidCollider.bounds.center : transform.position;
        Vector3 dirParaPorta = centroPorta - refPosicao.position;
        dirParaPorta.y = 0f;

        if (dirParaPorta.magnitude > distanciaInteracao) return false;
        if (dirParaPorta.sqrMagnitude < 0.001f) return true;

        dirParaPorta.Normalize();

        Vector3 frenteOlhar = refVisao.forward;
        frenteOlhar.y = 0f;
        if (frenteOlhar.sqrMagnitude < 0.001f) return false;
        frenteOlhar.Normalize();

        return Vector3.Dot(frenteOlhar, dirParaPorta) >= precisaoDeFrente;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;

        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        playerInZone = pm;
        playerRb = other.GetComponentInParent<Rigidbody>();

        CheckSpeedAndOpen(playerInZone, playerRb);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
        if (pm != null && pm == playerInZone)
        {
            playerInZone = null;
            playerRb = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isOpen) return;

        PlayerMovement pm = collision.gameObject.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;

        Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();
        CheckSpeedAndOpen(pm, rb);
    }

    private void CheckSpeedAndOpen(PlayerMovement pm, Rigidbody rb)
    {
        if (isOpen || pm == null) return;

        bool isDashing = pm.state == PlayerMovement.MovementState.dashing;
        bool isRunningFast = rb != null && rb.linearVelocity.magnitude >= minSpeedToBreak;

        if (isDashing || isRunningFast)
        {
            OpenDoor(pm.transform.position);
        }
    }

    public void OpenDoor(Vector3 playerPosition)
    {
        if (isOpen) return;
        isOpen = true;

        LibertarFoco();

        if (solidCollider != null)
            solidCollider.enabled = false;

        if (navObstacle != null)
            navObstacle.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Default");

        // Garante que a porta abra sempre para longe do lado em que o jogador está
        Vector3 toPlayer = playerPosition - transform.position;
        float playerSide = Vector3.Dot(normalAxis, toPlayer) >= 0f ? 1f : -1f;
        float sideMultiplier = hingeOnRight ? 1f : -1f;
        float swingSign = Mathf.Sign(Vector3.Dot(Vector3.Cross(Vector3.up, widthAxis), normalAxis));

        targetAngle = openAngle * playerSide * sideMultiplier * swingSign;

        float duracao = Mathf.Clamp(Mathf.Abs(targetAngle) / Mathf.Max(openSpeed, 60f), 0.15f, 0.5f);
        if (tweenAbertura.isAlive) tweenAbertura.Stop();

        tweenAbertura = Tween.Custom(
            currentAngle,
            targetAngle,
            duracao,
            novoAngulo =>
            {
                float delta = novoAngulo - currentAngle;
                transform.RotateAround(hingePoint, Vector3.up, delta);
                currentAngle = novoAngulo;
            },
            Ease.OutCubic
        );

        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position, soundVolume);
    }

    private void MostrarAviso(bool visivel)
    {
        if (textoAvisoUI == null) return;

        if (visivel)
        {
            textoAvisoUI.text = mensagemAbrir;
            if (!textoAvisoUI.gameObject.activeSelf)
            {
                textoAvisoUI.gameObject.SetActive(true);
            }
        }
        else if (textoAvisoUI.gameObject.activeSelf)
        {
            textoAvisoUI.gameObject.SetActive(false);
        }
    }

    private void LibertarFoco()
    {
        if (portaEmFoco == this)
        {
            portaEmFoco = null;
            MostrarAviso(false);
        }
    }

    private void OnDisable()
    {
        if (tweenAbertura.isAlive) tweenAbertura.Stop();
        LibertarFoco();
    }

    private void OnDrawGizmosSelected()
    {
        CalcularDobradica();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hingePoint, 0.12f);
    }
}