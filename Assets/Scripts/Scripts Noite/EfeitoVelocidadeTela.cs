using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class EfeitoVelocidadeTela : MonoBehaviour
{
    [Header("Detecção de Corrida Frontal")]
    [Tooltip("Velocidade horizontal mínima para ativar os traços na tela.")]
    [SerializeField] private float velocidadeMinima = 7.5f;
    [Tooltip("0.92 exige corrida reta para frente, cancelando os traços ao andar na diagonal ou para os lados.")]
    [SerializeField] [Range(0.5f, 0.99f)] private float limiarFrente = 0.92f;

    [Header("Aparência dos Traços")]
    [SerializeField] private int quantidadeTracos = 28;
    [SerializeField] private Color corTracos = new Color(1f, 1f, 1f, 0.32f);
    [Tooltip("Distância do centro da tela onde os traços começam (mantém a mira livre).")]
    [SerializeField] private float raioInterno = 440f;
    [Tooltip("Distância até onde os traços viajam antes de reiniciar.")]
    [SerializeField] private float raioExterno = 1150f;
    [SerializeField] private Vector2 comprimentoMinMax = new Vector2(180f, 380f);
    [SerializeField] private Vector2 espessuraMinMax = new Vector2(4f, 9f);

    [Header("Movimento e Transição (PrimeTween)")]
    [SerializeField] private float velocidadeTracos = 2600f;
    [SerializeField] private float duracaoFadeIn = 0.14f;
    [SerializeField] private float duracaoFadeOut = 0.08f;

    private struct TracoUI
    {
        public RectTransform rect;
        public Image imagem;
        public Vector2 direcao;
        public float distanciaAtual;
        public float velocidadeIndividual;
    }

    private PlayerMovement playerMovement;
    private Rigidbody playerRb;
    private CanvasGroup canvasGroup;
    private RectTransform container;
    private TracoUI[] tracos;

    private Texture2D texturaTraco;
    private Sprite spriteTraco;

    private Tween tweenAlpha;
    private bool efeitoAtivo = false;

    private void Awake()
    {
        container = GetComponent<RectTransform>();
        if (container == null)
        {
            container = gameObject.AddComponent<RectTransform>();
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        spriteTraco = CriarSpriteTracoAfilado();
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        BuscarJogador();
        GerarTracos();
    }

    private Sprite CriarSpriteTracoAfilado()
    {
        const int largura = 64;
        const int altura = 16;

        texturaTraco = new Texture2D(largura, altura, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int y = 0; y < altura; y++)
        {
            float v = Mathf.Abs((y / (float)(altura - 1)) * 2f - 1f);

            for (int x = 0; x < largura; x++)
            {
                float u = x / (float)(largura - 1);

                float larguraPerfil = Mathf.Sin(u * Mathf.PI) * Mathf.Pow(u, 0.35f);
                float alphaVertical = 1f - Mathf.SmoothStep(larguraPerfil * 0.45f, Mathf.Max(0.02f, larguraPerfil), v);
                float alphaCauda = Mathf.Pow(u, 0.75f);

                float alphaFinal = Mathf.Clamp01(alphaVertical * alphaCauda);
                texturaTraco.SetPixel(x, y, new Color(1f, 1f, 1f, alphaFinal));
            }
        }

        texturaTraco.Apply();
        return Sprite.Create(texturaTraco, new Rect(0f, 0f, largura, altura), new Vector2(0.5f, 0.5f));
    }

    private void BuscarJogador()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerRb = playerMovement.GetComponent<Rigidbody>();
        }
    }

    private void GerarTracos()
    {
        tracos = new TracoUI[quantidadeTracos];

        for (int i = 0; i < quantidadeTracos; i++)
        {
            GameObject obj = new GameObject($"TracoVelocidade_{i}", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(container, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image img = obj.GetComponent<Image>();
            img.sprite = spriteTraco;
            img.raycastTarget = false;

            tracos[i] = new TracoUI
            {
                rect = rect,
                imagem = img
            };

            ReiniciarTraco(ref tracos[i], espalharInicialmente: true);
        }
    }

    private void ReiniciarTraco(ref TracoUI traco, bool espalharInicialmente)
    {
        float comprimento = Random.Range(comprimentoMinMax.x, comprimentoMinMax.y);
        float espessura = Random.Range(espessuraMinMax.x, espessuraMinMax.y);
        traco.rect.sizeDelta = new Vector2(comprimento, espessura);

        Color corVariada = corTracos;
        corVariada.a *= Random.Range(0.45f, 1f);
        traco.imagem.color = corVariada;

        traco.velocidadeIndividual = velocidadeTracos * Random.Range(0.85f, 1.25f);

        float angulo = Random.Range(0f, 360f);
        float rad = angulo * Mathf.Deg2Rad;

        traco.direcao = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        traco.rect.localRotation = Quaternion.Euler(0f, 0f, angulo);

        traco.distanciaAtual = espalharInicialmente
            ? Random.Range(raioInterno, raioExterno)
            : raioInterno + Random.Range(0f, 120f);

        traco.rect.anchoredPosition = traco.direcao * traco.distanciaAtual;
    }

    private void Update()
    {
        if (tracos == null) return;

        if (playerMovement == null)
        {
            BuscarJogador();
            return;
        }

        bool deveAtivar = VerificarCorridaExclusivaParaFrente();

        if (deveAtivar != efeitoAtivo)
        {
            efeitoAtivo = deveAtivar;
            AnimarVisibilidade(efeitoAtivo);
        }

        if (canvasGroup.alpha > 0.01f)
        {
            AtualizarTracos();
        }
    }

    private bool VerificarCorridaExclusivaParaFrente()
    {
        if (playerRb == null) return false;

        // Só ativa durante a corrida (Shift / Botão Direito) quando estiver indo reto para frente
        if (playerMovement.state != PlayerMovement.MovementState.dashing)
        {
            return false;
        }

        Vector3 velHorizontal = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        if (velHorizontal.magnitude < velocidadeMinima)
        {
            return false;
        }

        Transform refFrente = playerMovement.orientation != null
            ? playerMovement.orientation
            : (Camera.main != null ? Camera.main.transform : playerMovement.transform);

        Vector3 frente = refFrente.forward;
        frente.y = 0f;
        if (frente.sqrMagnitude < 0.001f) return false;
        frente.Normalize();

        return Vector3.Dot(frente, velHorizontal.normalized) >= limiarFrente;
    }

    private void AnimarVisibilidade(bool mostrar)
    {
        if (tweenAlpha.isAlive) tweenAlpha.Stop();

        float alvo = mostrar ? 1f : 0f;
        float duracao = mostrar ? duracaoFadeIn : duracaoFadeOut;

        tweenAlpha = Tween.Alpha(canvasGroup, alvo, duracao, Ease.OutQuad);
    }

    private void AtualizarTracos()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < tracos.Length; i++)
        {
            tracos[i].distanciaAtual += tracos[i].velocidadeIndividual * dt;

            if (tracos[i].distanciaAtual >= raioExterno)
            {
                ReiniciarTraco(ref tracos[i], espalharInicialmente: false);
            }
            else
            {
                tracos[i].rect.anchoredPosition = tracos[i].direcao * tracos[i].distanciaAtual;
            }
        }
    }

    private void OnDisable()
    {
        if (tweenAlpha.isAlive) tweenAlpha.Stop();
    }

    private void OnDestroy()
    {
        if (spriteTraco != null) Destroy(spriteTraco);
        if (texturaTraco != null) Destroy(texturaTraco);
    }
}