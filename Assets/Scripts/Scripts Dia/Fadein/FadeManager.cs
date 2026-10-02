using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instancia;

    [Header("Interface")]
    public CanvasGroup fadePanel; // Arraste a sua TelaPreta para cá
    public float tempoFade = 1.5f;

    [Header("Cena da Noite")]
    public string nomeCenaNoite = "CenaShooter"; // Escreva o nome exato da sua cena da noite

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        // Ao iniciar o dia, faz o Fade In (do preto para transparente)
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            fadePanel.blocksRaycasts = true;
            StartCoroutine(FadeRotina(1f, 0f));
        }
    }

    public void TransicaoParaNoite()
    {
        StartCoroutine(FadeRotina(0f, 1f, true));
    }

    private IEnumerator FadeRotina(float inicio, float fim, bool carregarCena = false)
    {
        float tempoDecorrido = 0f;
        
        if (inicio == 0f) fadePanel.blocksRaycasts = true; 

        while (tempoDecorrido < tempoFade)
        {
            tempoDecorrido += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(inicio, fim, tempoDecorrido / tempoFade);
            yield return null;
        }

        fadePanel.alpha = fim;

        if (fim == 0f) fadePanel.blocksRaycasts = false; 

        if (carregarCena)
        {
            // Carrega a cena do boomer shooter
            SceneManager.LoadScene(nomeCenaNoite);
        }
    }
}