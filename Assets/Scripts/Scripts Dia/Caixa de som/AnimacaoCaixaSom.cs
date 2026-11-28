using UnityEngine;
using PrimeTween;

public class AnimacaoCaixaSom : MonoBehaviour
{
    [Header("Animação com Base Fixa (PrimeTween)")]
    [Tooltip("Quanto o topo da caixa estica para cima (0.06 = 6% da altura).")]
    [SerializeField] [Range(0.01f, 0.25f)] private float intensidadeEsticar = 0.06f;
    [Tooltip("Duração de cada subida ou descida (valores menores = batida mais rápida).")]
    [SerializeField] private float duracaoBatida = 0.28f;
    [Tooltip("Atraso inicial em segundos (ex: 0 na caixa esquerda e 0.14 na direita para alternarem no ritmo).")]
    [SerializeField] private float atrasoInicial = 0f;
    [Tooltip("Afina levemente as laterais quando estica para cima, dando efeito elástico de desenho.")]
    [SerializeField] private bool manterVolume = true;

    [Header("Sincronização com Áudio (Opcional)")]
    [Tooltip("Se preenchido, a animação só roda enquanto este AudioSource estiver tocando.")]
    [SerializeField] private AudioSource fonteAudio;

    private Vector3 posInicialLocal;
    private Vector3 escalaInicial;
    private float offsetBaseLocalY;
    private float fatorVerticalAtual = 1f;

    private Tween tweenBatida;
    private Tween tweenRetorno;
    private bool animando = false;

    private void Awake()
    {
        posInicialLocal = transform.localPosition;
        escalaInicial = transform.localScale;
        CalcularOffsetDaBase();
    }

    private void CalcularOffsetDaBase()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            offsetBaseLocalY = 0f;
            return;
        }

        // Calcula o ponto mais baixo do objeto no mundo (onde ele encosta no chão)
        Bounds boundsTotais = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            boundsTotais.Encapsulate(renderers[i].bounds);
        }

        Vector3 pontoBaseMundo = new Vector3(transform.position.x, boundsTotais.min.y, transform.position.z);
        Vector3 pontoBaseNoPai = transform.parent != null
            ? transform.parent.InverseTransformPoint(pontoBaseMundo)
            : pontoBaseMundo;

        // Distância entre o pivô do objeto e a base encostada no chão
        offsetBaseLocalY = pontoBaseNoPai.y - posInicialLocal.y;
    }

    private void OnEnable()
    {
        AplicarDeformacaoComBaseFixa(1f);

        if (fonteAudio == null || fonteAudio.isPlaying)
        {
            IniciarAnimacao();
        }
    }

    private void Update()
    {
        if (fonteAudio == null) return;

        if (fonteAudio.isPlaying && !animando)
        {
            IniciarAnimacao();
        }
        else if (!fonteAudio.isPlaying && animando)
        {
            PararAnimacao();
        }
    }

    public void IniciarAnimacao()
    {
        if (animando) return;
        animando = true;

        PararTweens();

        tweenBatida = Tween.Custom(
            startValue: 1f,
            endValue: 1f + intensidadeEsticar,
            duration: duracaoBatida,
            onValueChange: valor => AplicarDeformacaoComBaseFixa(valor),
            ease: Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo,
            startDelay: atrasoInicial
        );
    }

    public void PararAnimacao()
    {
        if (!animando) return;
        animando = false;

        PararTweens();

        tweenRetorno = Tween.Custom(
            startValue: fatorVerticalAtual,
            endValue: 1f,
            duration: 0.2f,
            onValueChange: valor => AplicarDeformacaoComBaseFixa(valor),
            ease: Ease.OutQuad
        );
    }

    private void AplicarDeformacaoComBaseFixa(float fatorY)
    {
        fatorVerticalAtual = fatorY;

        float fatorLateral = manterVolume ? 1f - ((fatorY - 1f) * 0.35f) : 1f;

        transform.localScale = new Vector3(
            escalaInicial.x * fatorLateral,
            escalaInicial.y * fatorY,
            escalaInicial.z * fatorLateral
        );

        // Compensa a posição Y do pivô na mesma proporção da escala para manter a base imóvel
        float novaPosY = posInicialLocal.y + offsetBaseLocalY * (1f - fatorY);
        transform.localPosition = new Vector3(posInicialLocal.x, novaPosY, posInicialLocal.z);
    }

    private void PararTweens()
    {
        if (tweenBatida.isAlive) tweenBatida.Stop();
        if (tweenRetorno.isAlive) tweenRetorno.Stop();
    }

    private void OnDisable()
    {
        animando = false;
        PararTweens();
    }
}