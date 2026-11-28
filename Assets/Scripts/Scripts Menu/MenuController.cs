using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PrimeTween;

public class MenuController : MonoBehaviour
{
    [Header("Menu Principal")]
    public RectTransform titulo;
    public RectTransform grupoBotoes;
    public CanvasGroup canvasGroupBotoes;

    [Header("Fundo Escurecido (Opcional)")]
    [Tooltip("Painel escuro atrás das janelas pop-up para destacar a janela aberta.")]
    public CanvasGroup fundoEscurecido;

    [Header("Janela de Opções")]
    public RectTransform painelOpcoes;
    public CanvasGroup canvasGroupOpcoes;
    [Tooltip("Arraste aqui o botão 'X' do canto da janela de Opções.")]
    public Button botaoFecharOpcoes;

    [Header("Controles de Opções (Opcional)")]
    public Slider sliderVolume;
    public TextMeshProUGUI textoVolume;
    public Slider sliderSensibilidade;
    public TextMeshProUGUI textoSensibilidade;
    public Toggle toggleTelaCheia;

    [Header("Janela de Créditos")]
    [Tooltip("Se deixar vazio, uma janela placeholder com botão 'X' será criada automaticamente.")]
    public RectTransform painelCreditos;
    public CanvasGroup canvasGroupCreditos;
    [Tooltip("Arraste aqui o botão 'X' do canto da janela de Créditos quando criar a definitiva.")]
    public Button botaoFecharCreditos;

    [Header("Configurações")]
    public string nomeCenaDoJogo = "CafeDoUrso_Dia";

    private Tween tweenJanela;
    private Tween tweenFundo;

    private const string PREF_VOLUME = "VolumeGeral";
    private const string PREF_SENSIBILIDADE = "SensibilidadeMouse";
    private const string PREF_TELA_CHEIA = "TelaCheia";

    private void Start()
    {
        // Cria automaticamente a janelinha placeholder caso você ainda não tenha montado a de Créditos
        if (painelCreditos == null)
        {
            CriarPlaceholderCreditos();
        }

        ConfigurarBotoesFechar();
        ConfigurarControlesOpcoes();
        PrepararEstadoInicial();

        // Animação de entrada do Menu Principal
        if (titulo != null)
            Tween.UIAnchoredPosition(titulo, new Vector2(0f, 150f), duration: 1.2f, ease: Ease.OutBounce);

        if (grupoBotoes != null)
            Tween.Scale(grupoBotoes, Vector3.one, duration: 0.8f, ease: Ease.OutBack, startDelay: 0.5f);
    }

    private void ConfigurarBotoesFechar()
    {
        if (botaoFecharOpcoes != null)
        {
            botaoFecharOpcoes.onClick.RemoveListener(BotaoVoltarOpcoes);
            botaoFecharOpcoes.onClick.AddListener(BotaoVoltarOpcoes);
        }

        if (botaoFecharCreditos != null)
        {
            botaoFecharCreditos.onClick.RemoveListener(BotaoVoltarCreditos);
            botaoFecharCreditos.onClick.AddListener(BotaoVoltarCreditos);
        }
    }

    private void PrepararEstadoInicial()
    {
        if (titulo != null)
            titulo.anchoredPosition = new Vector2(titulo.anchoredPosition.x, 800f);

        if (grupoBotoes != null)
            grupoBotoes.localScale = Vector3.zero;

        if (canvasGroupBotoes != null)
        {
            canvasGroupBotoes.interactable = true;
            canvasGroupBotoes.blocksRaycasts = true;
        }

        if (fundoEscurecido != null)
        {
            fundoEscurecido.alpha = 0f;
            fundoEscurecido.blocksRaycasts = false;
            fundoEscurecido.interactable = false;
            fundoEscurecido.gameObject.SetActive(false);
        }

        InicializarJanela(painelOpcoes, canvasGroupOpcoes);
        InicializarJanela(painelCreditos, canvasGroupCreditos);
    }

    private void InicializarJanela(RectTransform painel, CanvasGroup group)
    {
        if (painel == null) return;

        painel.localScale = Vector3.zero;
        if (group != null)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        painel.gameObject.SetActive(false);
    }

    // ================= CONTROLES DE OPÇÕES =================
    private void ConfigurarControlesOpcoes()
    {
        float volumeSalvo = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
        float sensibilidadeSalva = PlayerPrefs.GetFloat(PREF_SENSIBILIDADE, 1f);
        bool telaCheiaSalva = PlayerPrefs.GetInt(PREF_TELA_CHEIA, Screen.fullScreen ? 1 : 0) == 1;

        AudioListener.volume = volumeSalvo;
        Screen.fullScreen = telaCheiaSalva;

        if (sliderVolume != null)
        {
            sliderVolume.minValue = 0f;
            sliderVolume.maxValue = 1f;
            sliderVolume.value = volumeSalvo;
            AtualizarTextoVolume(volumeSalvo);
            sliderVolume.onValueChanged.AddListener(AoMudarVolume);
        }

        if (sliderSensibilidade != null)
        {
            sliderSensibilidade.minValue = 0.2f;
            sliderSensibilidade.maxValue = 3f;
            sliderSensibilidade.value = sensibilidadeSalva;
            AtualizarTextoSensibilidade(sensibilidadeSalva);
            sliderSensibilidade.onValueChanged.AddListener(AoMudarSensibilidade);
        }

        if (toggleTelaCheia != null)
        {
            toggleTelaCheia.isOn = telaCheiaSalva;
            toggleTelaCheia.onValueChanged.AddListener(AoMudarTelaCheia);
        }
    }

    public void AoMudarVolume(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat(PREF_VOLUME, valor);
        PlayerPrefs.Save();
        AtualizarTextoVolume(valor);
    }

    public void AoMudarSensibilidade(float valor)
    {
        PlayerPrefs.SetFloat(PREF_SENSIBILIDADE, valor);
        PlayerPrefs.Save();
        AtualizarTextoSensibilidade(valor);
    }

    public void AoMudarTelaCheia(bool ativo)
    {
        Screen.fullScreen = ativo;
        PlayerPrefs.SetInt(PREF_TELA_CHEIA, ativo ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void AtualizarTextoVolume(float valor)
    {
        if (textoVolume != null)
            textoVolume.text = Mathf.RoundToInt(valor * 100f) + "%";
    }

    private void AtualizarTextoSensibilidade(float valor)
    {
        if (textoSensibilidade != null)
            textoSensibilidade.text = valor.ToString("0.0") + "x";
    }

    // ================= NAVEGAÇÃO PRINCIPAL =================
    public void BotaoJogar()
    {
        if (canvasGroupBotoes != null) canvasGroupBotoes.interactable = false;
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() => SceneManager.LoadScene(nomeCenaDoJogo));
    }

    // ================= POP-UP DE OPÇÕES =================
    public void BotaoOpcoes()
    {
        AbrirJanelaSobreposta(painelOpcoes, canvasGroupOpcoes);
    }

    public void BotaoVoltarOpcoes()
    {
        FecharJanelaSobreposta(painelOpcoes, canvasGroupOpcoes);
    }

    // ================= POP-UP DE CRÉDITOS =================
    public void BotaoCreditos()
    {
        AbrirJanelaSobreposta(painelCreditos, canvasGroupCreditos);
    }

    public void BotaoVoltarCreditos()
    {
        FecharJanelaSobreposta(painelCreditos, canvasGroupCreditos);
    }

    // Método único caso prefira ligar qualquer botão "X" diretamente no OnClick()
    public void BotaoFecharJanelaAtiva()
    {
        if (painelOpcoes != null && painelOpcoes.gameObject.activeSelf)
        {
            BotaoVoltarOpcoes();
        }
        else if (painelCreditos != null && painelCreditos.gameObject.activeSelf)
        {
            BotaoVoltarCreditos();
        }
    }

    // ================= LÓGICA DO POP-UP SOBREPOSTO =================
    private void AbrirJanelaSobreposta(RectTransform painel, CanvasGroup group)
    {
        if (painel == null) return;

        if (canvasGroupBotoes != null)
        {
            canvasGroupBotoes.interactable = false;
            canvasGroupBotoes.blocksRaycasts = false;
        }

        if (tweenFundo.isAlive) tweenFundo.Stop();
        if (fundoEscurecido != null)
        {
            fundoEscurecido.gameObject.SetActive(true);
            fundoEscurecido.blocksRaycasts = true;
            tweenFundo = Tween.Alpha(fundoEscurecido, 1f, duration: 0.25f, ease: Ease.OutQuad);
        }

        if (tweenJanela.isAlive) tweenJanela.Stop();
        painel.gameObject.SetActive(true);
        painel.SetAsLastSibling();
        painel.localScale = Vector3.zero;

        tweenJanela = Tween.Scale(painel, Vector3.one, duration: 0.35f, ease: Ease.OutBack)
            .OnComplete(() =>
            {
                if (group != null)
                {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }
            });
    }

    private void FecharJanelaSobreposta(RectTransform painel, CanvasGroup group)
    {
        if (painel == null) return;

        if (group != null)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (tweenFundo.isAlive) tweenFundo.Stop();
        if (fundoEscurecido != null)
        {
            tweenFundo = Tween.Alpha(fundoEscurecido, 0f, duration: 0.2f, ease: Ease.InQuad)
                .OnComplete(() =>
                {
                    fundoEscurecido.blocksRaycasts = false;
                    fundoEscurecido.gameObject.SetActive(false);
                });
        }

        if (tweenJanela.isAlive) tweenJanela.Stop();
        tweenJanela = Tween.Scale(painel, Vector3.zero, duration: 0.25f, ease: Ease.InBack)
            .OnComplete(() =>
            {
                painel.gameObject.SetActive(false);

                if (canvasGroupBotoes != null)
                {
                    canvasGroupBotoes.interactable = true;
                    canvasGroupBotoes.blocksRaycasts = true;
                }
            });
    }

    // ================= PLACEHOLDER AUTOMÁTICO DE CRÉDITOS =================
    private void CriarPlaceholderCreditos()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject janelaObj = new GameObject("PainelCreditos_Placeholder", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        janelaObj.transform.SetParent(canvas.transform, false);

        RectTransform rectJanela = janelaObj.GetComponent<RectTransform>();
        rectJanela.anchorMin = new Vector2(0.5f, 0.5f);
        rectJanela.anchorMax = new Vector2(0.5f, 0.5f);
        rectJanela.sizeDelta = new Vector2(620f, 380f);
        rectJanela.anchoredPosition = Vector2.zero;

        Image imgJanela = janelaObj.GetComponent<Image>();
        imgJanela.color = new Color(0.11f, 0.11f, 0.14f, 0.96f);

        CanvasGroup cgJanela = janelaObj.GetComponent<CanvasGroup>();

        // Botão "X" no canto superior direito
        GameObject btnObj = new GameObject("BotaoFecharX", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(janelaObj.transform, false);
        RectTransform rectBtn = btnObj.GetComponent<RectTransform>();
        rectBtn.anchorMin = new Vector2(1f, 1f);
        rectBtn.anchorMax = new Vector2(1f, 1f);
        rectBtn.pivot = new Vector2(1f, 1f);
        rectBtn.sizeDelta = new Vector2(42f, 42f);
        rectBtn.anchoredPosition = new Vector2(-14f, -14f);

        Image imgBtn = btnObj.GetComponent<Image>();
        imgBtn.color = new Color(0.85f, 0.32f, 0.28f, 1f);

        Button btn = btnObj.GetComponent<Button>();

        GameObject txtBtnObj = new GameObject("TextoX", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtBtnObj.transform.SetParent(btnObj.transform, false);
        RectTransform rectTxtBtn = txtBtnObj.GetComponent<RectTransform>();
        rectTxtBtn.anchorMin = Vector2.zero;
        rectTxtBtn.anchorMax = Vector2.one;
        rectTxtBtn.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmpBtn = txtBtnObj.GetComponent<TextMeshProUGUI>();
        tmpBtn.text = "X";
        tmpBtn.fontSize = 24f;
        tmpBtn.fontStyle = FontStyles.Bold;
        tmpBtn.alignment = TextAlignmentOptions.Center;
        tmpBtn.color = Color.white;

        // Título
        GameObject tituloObj = new GameObject("TituloCreditos", typeof(RectTransform), typeof(TextMeshProUGUI));
        tituloObj.transform.SetParent(janelaObj.transform, false);
        RectTransform rectTitulo = tituloObj.GetComponent<RectTransform>();
        rectTitulo.anchorMin = new Vector2(0.5f, 1f);
        rectTitulo.anchorMax = new Vector2(0.5f, 1f);
        rectTitulo.sizeDelta = new Vector2(460f, 55f);
        rectTitulo.anchoredPosition = new Vector2(0f, -45f);

        TextMeshProUGUI tmpTitulo = tituloObj.GetComponent<TextMeshProUGUI>();
        tmpTitulo.text = "CRÉDITOS";
        tmpTitulo.fontSize = 34f;
        tmpTitulo.fontStyle = FontStyles.Bold;
        tmpTitulo.alignment = TextAlignmentOptions.Center;
        tmpTitulo.color = Color.white;

        // Lista Placeholder
        GameObject listaObj = new GameObject("ListaCreditos", typeof(RectTransform), typeof(TextMeshProUGUI));
        listaObj.transform.SetParent(janelaObj.transform, false);
        RectTransform rectLista = listaObj.GetComponent<RectTransform>();
        rectLista.anchorMin = new Vector2(0.5f, 0.5f);
        rectLista.anchorMax = new Vector2(0.5f, 0.5f);
        rectLista.sizeDelta = new Vector2(540f, 230f);
        rectLista.anchoredPosition = new Vector2(0f, -20f);

        TextMeshProUGUI tmpLista = listaObj.GetComponent<TextMeshProUGUI>();
        tmpLista.text = "<b>Equipe de Desenvolvimento</b>\n\n" +
                        "• Game Design & Programação — Nome 1\n" +
                        "• Arte 2D / 3D & UI — Nome 2\n" +
                        "• Áudio & Trilha Sonora — Nome 3\n\n" +
                        "<size=80%><color=#AAAAAA>(Tela temporária - substituir pelo painel final)</color></size>";
        tmpLista.fontSize = 22f;
        tmpLista.alignment = TextAlignmentOptions.Center;
        tmpLista.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        painelCreditos = rectJanela;
        canvasGroupCreditos = cgJanela;
        botaoFecharCreditos = btn;
    }

    // ================= SAIR =================
    public void BotaoSair()
    {
        if (canvasGroupBotoes != null) canvasGroupBotoes.interactable = false;
        Tween.Scale(grupoBotoes, Vector3.zero, duration: 0.4f, ease: Ease.InBack)
            .OnComplete(() =>
            {
                Debug.Log("A sair do Jogo...");
                Application.Quit();
            });
    }
}