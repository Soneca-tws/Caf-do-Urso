using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class SimpleDoor : MonoBehaviour
{
    [Header("Configuração de Abertura")]
    [Tooltip("Velocidade mínima para abrir correndo.")]
    [SerializeField] private float minSpeedToBreak = 4.5f;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float openSpeed = 550f;
    [SerializeField] private bool hingeOnRight = false;

    [Header("Áudio (Opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private float soundVolume = 1f;

    private BoxCollider solidCollider;
    private NavMeshObstacle navObstacle;
    private PlayerMovement playerInZone;
    private Rigidbody playerRb;

    private bool isOpen = false;
    private Vector3 hingePoint;
    private Vector3 widthAxis;
    private Vector3 normalAxis;
    private float currentAngle = 0f;
    private float targetAngle = 0f;

    private void Awake()
    {
        navObstacle = GetComponent<NavMeshObstacle>();
        CalcularDobradica();
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
        if (isOpen)
        {
            if (Mathf.Abs(currentAngle - targetAngle) > 0.01f)
            {
                float nextAngle = Mathf.MoveTowards(currentAngle, targetAngle, openSpeed * Time.deltaTime);
                float delta = nextAngle - currentAngle;
                transform.RotateAround(hingePoint, Vector3.up, delta);
                currentAngle = nextAngle;
            }
            return;
        }

        if (playerInZone == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenDoor(playerInZone.transform.position);
            return;
        }

        CheckSpeedAndOpen(playerInZone, playerRb);
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

        if (openSound != null)
            AudioSource.PlayClipAtPoint(openSound, transform.position, soundVolume);
    }

    private void OnDrawGizmosSelected()
    {
        CalcularDobradica();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hingePoint, 0.12f);
    }
}