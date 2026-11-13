using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;

public class MenuController : MonoBehaviour
{
    [Header("Menu Principal")]
    public RectTransform titulo;
    public RectTransform grupoBotoes;
    public CanvasGroup canvasGroupBotoes; 
    
    [Header("Painel de Opções")]
    public RectTransform painelOpcoes; 
    public CanvasGroup canvasGroupOpcoes; 

    [Header("Painel de Créditos")]
    public RectTransform painelCreditos; 
    public CanvasGroup canvasGroupCreditos; 

    [Header("Configurações")]
    public string nomeCenaDoJogo = "CafeDoUrso_Dia"; 

    private void Start()
    {
        // 1. Esconde tudo no primeiro frame
        titulo.anchoredPosition = new Vector2(titulo.anchoredPosition.x, 800f); 
        grupoBotoes.localScale = Vector3.zero; 
        
        // Esconde Opções
        painelOpcoes.localScale = Vector3.zero;
        if (canvasGroupOpcoes != null) canvasGroupOpcoes.interactable = false;

        // Esconde Créditos
        painelCreditos.localScale = Vector3.zero;
        if (canvasGroupCreditos != null) canvasGroupCreditos.interactable = false;

        // 2. Animação de Entrada do Menu Principal
        Tween.UIAnchoredPosition(titulo, new Vector2(0, 150f), duration: 1.2f, ease: Ease.OutBounce);
        Tween.Scale(grupoBotoes, Vector3.one, duration: 0.8f, ease: Ease.OutBack, startDelay: 0.5f);
    }

    public void BotaoJogar()
    {
        canvasGroupBotoes.interactable = false;
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() => SceneManager.LoadScene(nomeCenaDoJogo));
    }

    // ================= ANIMAÇÃO DAS OPÇÕES =================
    public void BotaoOpcoes()
    {
        canvasGroupBotoes.interactable = false;
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack).OnComplete(() => 
        {
            Tween.Scale(painelOpcoes, Vector3.one, duration: 0.6f, ease: Ease.OutBack);
            if (canvasGroupOpcoes != null) canvasGroupOpcoes.interactable = true;
        });
    }

    public void BotaoVoltarOpcoes()
    {
        if (canvasGroupOpcoes != null) canvasGroupOpcoes.interactable = false;
        Tween.Scale(painelOpcoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack).OnComplete(() => 
        {
            Tween.Scale(grupoBotoes, Vector3.one, duration: 0.6f, ease: Ease.OutBack);
            canvasGroupBotoes.interactable = true;
        });
    }

    // ================= ANIMAÇÃO DOS CRÉDITOS =================
    public void BotaoCreditos()
    {
        canvasGroupBotoes.interactable = false; 
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack).OnComplete(() => 
        {
            Tween.Scale(painelCreditos, Vector3.one, duration: 0.6f, ease: Ease.OutBack);
            if (canvasGroupCreditos != null) canvasGroupCreditos.interactable = true;
        });
    }

    public void BotaoVoltarCreditos()
    {
        if (canvasGroupCreditos != null) canvasGroupCreditos.interactable = false;
        Tween.Scale(painelCreditos, Vector3.zero, duration: 0.4f, ease: Ease.InBack).OnComplete(() => 
        {
            Tween.Scale(grupoBotoes, Vector3.one, duration: 0.6f, ease: Ease.OutBack);
            canvasGroupBotoes.interactable = true;
        });
    }

    // ================= SAIR =================
    public void BotaoSair()
    {
        canvasGroupBotoes.interactable = false;
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() => 
            {
                Debug.Log("A sair do Jogo...");
                Application.Quit();
            });
    }
}