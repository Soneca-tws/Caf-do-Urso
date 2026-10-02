using UnityEngine;

public class EfeitoHover : MonoBehaviour
{
    [Header("Configurações do Efeito")]
    public float alturaLevitar = 0.2f; // O quanto o objeto sobe
    public float aumentoTamanho = 1.1f; // 1.1 significa que ele fica 10% maior
    public float velocidadeAnimacao = 10f;

    private Vector3 posicaoOriginal;
    private Vector3 escalaOriginal;
    
    private Vector3 posicaoAlvo;
    private Vector3 escalaAlvo;

    private void Start()
    {
        // Salva a posição e tamanho iniciais assim que o jogo começa
        posicaoOriginal = transform.localPosition;
        escalaOriginal = transform.localScale;
        
        posicaoAlvo = posicaoOriginal;
        escalaAlvo = escalaOriginal;
    }

    // A Unity chama essa função automaticamente quando o mouse ENTRA no objeto
    private void OnMouseEnter()
    {
        posicaoAlvo = posicaoOriginal + new Vector3(0, alturaLevitar, 0);
        escalaAlvo = escalaOriginal * aumentoTamanho;
    }

    // A Unity chama essa função automaticamente quando o mouse SAI do objeto
    private void OnMouseExit()
    {
        posicaoAlvo = posicaoOriginal;
        escalaAlvo = escalaOriginal;
    }

    private void Update()
    {
        // Move e muda o tamanho suavemente em direção ao alvo
        transform.localPosition = Vector3.Lerp(transform.localPosition, posicaoAlvo, Time.deltaTime * velocidadeAnimacao);
        transform.localScale = Vector3.Lerp(transform.localScale, escalaAlvo, Time.deltaTime * velocidadeAnimacao);
    }
}