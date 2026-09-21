using UnityEngine;

public class CoffeeCollectible : MonoBehaviour
{
    [Header("Animação Retrô")]
    public float rotationSpeed = 90f; // Velocidade do giro
    public float floatSpeed = 2f;     // Velocidade da flutuação
    public float floatHeight = 0.2f;  // Altura máxima da flutuação

    private Vector3 startPos;

    [Header("Configurações do Item")]
    public int coffeeAmount = 1; // Quantas sacas isso vale?
    
    // Opcional: Adicione um efeito de partículas de poeira marrom ao pegar
    public GameObject collectEffect; 

    private void Start()
    {
        // Salva a posição inicial para a flutuação não fazer o item sair voando pelo mapa
        startPos = transform.position;
    }

    private void Update()
    {
        // 1. Faz a saca girar no próprio eixo Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // 2. Faz a saca flutuar para cima e para baixo suavemente usando seno (Mathf.Sin)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

   private void OnTriggerEnter(Collider other)
{
    // Isso vai escrever no Console o NOME e a TAG de qualquer coisa que encostar no cubo
    Debug.Log("Algo encostou no café: " + other.gameObject.name + " | Tag atual: " + other.tag);

    if (other.CompareTag("Player"))
    {
        ColetarCafe();
    }
}

    private void ColetarCafe()
    {
        Debug.Log("Saca de café coletada! Estoque + " + coffeeAmount);

        // Aqui você chamaria o script do inventário do jogador. Exemplo:
        // PlayerInventory inventario = FindObjectOfType<PlayerInventory>();
        // inventario.AdicionarCafe(coffeeAmount);

        // Instancia o efeito visual de coleta, se houver
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // TOCA UM SOM AQUI (ex: som de grãos chacoalhando)

        // Desativa a saca do mapa (melhor que Destroy, pois permite resetar o mapa depois)
        gameObject.SetActive(false);
    }
}