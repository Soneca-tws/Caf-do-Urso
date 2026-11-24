using System;
using UnityEngine;

public class ClienteMovimento : MonoBehaviour
{
    public event Action<ClienteMovimento> OnClienteSaiu;

    [Header("Caminho do Cliente (Opcional - busca automático por nome)")]
    public Transform pontoPorta;
    public Transform pontoBalcao;
    public float velocidade = 3f;

    [Header("Interação e Visual (Opcional - busca automático)")]
    public DialogueTrigger gatilhoDialogo;
    public DialogueRunner runner;
    public NPCVisualController visualController;

    private bool chegouNoBalcao = false;
    private bool indoEmbora = false;
    private bool jaPegouCafe = false;

    private void Awake()
    {
        if (visualController == null)
            visualController = GetComponentInChildren<NPCVisualController>();

        if (gatilhoDialogo == null)
            gatilhoDialogo = GetComponent<DialogueTrigger>();

        if (runner == null)
            runner = FindFirstObjectByType<DialogueRunner>();

        if (pontoPorta == null)
        {
            GameObject objPorta = GameObject.Find("Ponto_Porta");
            if (objPorta != null) pontoPorta = objPorta.transform;
        }

        if (pontoBalcao == null)
        {
            GameObject objBalcao = GameObject.Find("Ponto_Balcao");
            if (objBalcao != null) pontoBalcao = objBalcao.transform;
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
                // Se existir um TurnoDiaManager controlando a fila, avisa ele;
                // caso contrário, mantém o comportamento antigo de ir para a noite.
                if (OnClienteSaiu != null)
                {
                    OnClienteSaiu.Invoke(this);
                }
                else if (FadeManager.Instancia != null)
                {
                    FadeManager.Instancia.TransicaoParaNoite();
                }

                gameObject.SetActive(false);
            }
        }
    }

    public void ReceberCafe()
    {
        jaPegouCafe = true;

        if (visualController != null)
        {
            visualController.MudarEstado(EstadoNPC.Experimentando);
        }

        CameraTransition camera = FindAnyObjectByType<CameraTransition>();
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