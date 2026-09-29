using UnityEngine;

public class ClienteMovimento : MonoBehaviour
{
    [Header("Caminho do Cliente")]
    public Transform pontoPorta;
    public Transform pontoBalcao;
    public float velocidade = 3f;

    [Header("Interação")]
    [Tooltip("Arraste o objeto que tem o script DialogueTrigger para cá")]
    public DialogueTrigger gatilhoDialogo; 

    private bool chegouNoBalcao = false;

    private void Start()
    {
        // Posiciona o cliente na porta no início do jogo
        if (pontoPorta != null)
        {
            transform.position = pontoPorta.position;
        }
    }

    private void Update()
    {
        if (!chegouNoBalcao && pontoBalcao != null)
        {
            // Move a cápsula em direção ao balcão
            transform.position = Vector3.MoveTowards(transform.position, pontoBalcao.position, velocidade * Time.deltaTime);

            // Verifica se chegou
            if (Vector3.Distance(transform.position, pontoBalcao.position) < 0.1f)
            {
                chegouNoBalcao = true;
                
                // Dispara o diálogo automaticamente!
                if (gatilhoDialogo != null) 
                {
                    gatilhoDialogo.IniciarConversa();
                }
            }
        }
    }
}