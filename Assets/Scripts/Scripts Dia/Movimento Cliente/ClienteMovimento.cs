using UnityEngine;

public class ClienteMovimento : MonoBehaviour
{
    [Header("Caminho do Cliente")]
    public Transform pontoPorta;
    public Transform pontoBalcao;
    public float velocidade = 3f;

    [Header("Interação")]
    public DialogueTrigger gatilhoDialogo; 
    public DialogueRunner runner; 

    private bool chegouNoBalcao = false;
    private bool indoEmbora = false; 
    private bool jaPegouCafe = false; // NOVO: Impede que ele fuja antes da hora!

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

    // Função para avisar o cliente que a bebida foi entregue
    public void ReceberCafe()
    {
        jaPegouCafe = true;

        // NOVO: Nós avisamos a câmara BEM ANTES do último diálogo acabar!
        // Assim, quando o diálogo terminar, a câmara já vai estar travada.
        CameraTransition camera = Object.FindAnyObjectByType<CameraTransition>();
        if (camera != null)
        {
            camera.FinalizarExpediente();
        }
    }

    public void IniciarSaida()
    {
        // Agora o cliente apenas vira as costas e vai embora
        if (jaPegouCafe)
        {
            indoEmbora = true;
        }
    }
}