using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween; // Importação obrigatória do PrimeTween

public class MenuController : MonoBehaviour
{
    [Header("Elementos da Interface")]
    public RectTransform titulo;
    public RectTransform grupoBotoes;
    
    [Header("Configurações")]
    public string nomeCenaDoJogo = "CafeDoUrso_Dia"; // Coloque o nome exato da sua cena aqui

    private void Start()
    {
        // 1. Esconde tudo no primeiro frame para a animação começar do zero
        // Coloca o título bem acima da tela
        titulo.anchoredPosition = new Vector2(titulo.anchoredPosition.x, 800f); 
        // Encolhe os botões para o tamanho zero
        grupoBotoes.localScale = Vector3.zero; 

        // 2. Animação de Entrada
        // O título "cai" até a posição Y = 150 usando um efeito de quique (OutBounce)
        Tween.UIAnchoredPosition(titulo, new Vector2(0, 150f), duration: 1.2f, ease: Ease.OutBounce);
        
        // Os botões "crescem" até o tamanho normal (Vector3.one) com um leve atraso (.SetDelay)
        Tween.Scale(grupoBotoes, Vector3.one, duration: 0.8f, ease: Ease.OutBack, startDelay: 0.5f);
    }

    public void BotaoJogar()
    {
        // Trava os cliques para o jogador não apertar duas vezes durante a animação
        grupoBotoes.GetComponent<CanvasGroup>().interactable = false;

        // Animação de Saída: Os botões encolhem rápido...
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() => 
            {
                // ... e só quando a animação terminar, a cena do jogo carrega!
                SceneManager.LoadScene(nomeCenaDoJogo);
            });
    }

    public void BotaoSair()
    {
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() => 
            {
                Debug.Log("Saindo do Jogo...");
                Application.Quit();
            });
    }
}