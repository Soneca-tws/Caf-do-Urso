using UnityEngine;

public class ClienteMovimento : MonoBehaviour
{
    [Header("Caminho do Cliente")]
    public Transform pontoPorta;
    public Transform pontoBalcao;
    public float velocidade = 3f;

    [Header("Interação")]
    [Tooltip("Arraste o botão de iniciar diálogo para cá")]
    public GameObject botaoFalar; 

    private bool chegouNoBalcao = false;

    private void Start()
    {
        // Posiciona o cliente na porta no início do jogo
        if (pontoPorta != null)
        {
            transform.position = pontoPorta.position;
        }
        
        // Esconde o botão de falar até o cliente chegar
        if (botaoFalar != null) 
        {
            botaoFalar.SetActive(false);
        }
    }

    private void Update()
    {
        if (!chegouNoBalcao && pontoBalcao != null)
        {
            // Move a cápsula um pouco a cada frame na direção do balcão
            transform.position = Vector3.MoveTowards(transform.position, pontoBalcao.position, velocidade * Time.deltaTime);

            // Calcula a distância. Se for menor que 0.1, ele chegou
            if (Vector3.Distance(transform.position, pontoBalcao.position) < 0.1f)
            {
                chegouNoBalcao = true;
                
                // Mostra o botão para o jogador poder atender
                if (botaoFalar != null) 
                {
                    botaoFalar.SetActive(true);
                }
            }
        }
    }
}