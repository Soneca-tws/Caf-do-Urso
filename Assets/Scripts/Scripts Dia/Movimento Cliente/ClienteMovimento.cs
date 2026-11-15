using UnityEngine;

public class ClienteMovimento : MonoBehaviour
{
    [Header("Caminho do Cliente")]
    public Transform pontoPorta;
    public Transform pontoBalcao;
    public float velocidade = 3f;

    [Header("Interação e Visual")]
    public DialogueTrigger gatilhoDialogo;
    public DialogueRunner runner;
    public NPCVisualController visualController;

    private bool chegouNoBalcao = false;
    private bool indoEmbora = false;
    private bool jaPegouCafe = false;

    private void Awake()
    {
        // Se esquecer de arrastar no Inspector, pega automaticamente no próprio NPC
        if (visualController == null)
        {
            visualController = GetComponent<NPCVisualController>();
        }
    }

    private void OnEnable()
    {
        if (runner != null) runner.OnDialogueEnded += IniciarSaida;
    }

    private void OnDisable()
    {
        if (runner != null) runner.OnDialogueEnded -= IniciarSaida;
    }

    private void Start()
    {
        if (pontoPorta != null) transform.position = pontoPorta.position;

        if (visualController != null)
        {
            visualController.MudarEstado(EstadoNPC.Andando);
        }
    }

    private void Update()
    {
        if (!chegouNoBalcao && !indoEmbora && pontoBalcao != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, pontoBalcao.position, velocidade * Time.deltaTime);

            if (Vector3.Distance(transform.position, pontoBalcao.position) < 0.5f)
            {
                chegouNoBalcao = true;
                if (gatilhoDialogo != null) gatilhoDialogo.IniciarConversa();
            }
        }
        else if (indoEmbora && pontoPorta != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, pontoPorta.position, velocidade * Time.deltaTime);

            if (Vector3.Distance(transform.position, pontoPorta.position) < 0.5f)
            {
                if (FadeManager.Instancia != null) FadeManager.Instancia.TransicaoParaNoite();
                gameObject.SetActive(false);
            }
        }
    }

    public void ReceberCafe()
    {
        jaPegouCafe = true;

        // Muda para Experimentando enquanto o cliente prova a bebida antes da reação
        if (visualController != null)
        {
            visualController.MudarEstado(EstadoNPC.Experimentando);
        }

        CameraTransition camera = Object.FindAnyObjectByType<CameraTransition>();
        if (camera != null)
        {
            camera.FinalizarExpediente();
        }
    }

    public void IniciarSaida()
    {
        if (jaPegouCafe)
        {
            indoEmbora = true;

            if (visualController != null)
            {
                visualController.MudarEstado(EstadoNPC.Andando);
            }
        }
    }
}