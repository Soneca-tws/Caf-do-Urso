using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public enum EstadoInimigo
{
    Andando,
    Atacando,
    LevandoDano,
    Agarrado,
    Morto
}

[System.Serializable]
public class GrupoSpritesInimigo
{
    public EstadoInimigo estado;
    public Sprite[] frames;
    public float fps = 8f;
    public bool loop = true;
}

public class EnemyVisualController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Biblioteca de Estados")]
    [SerializeField] private List<GrupoSpritesInimigo> bibliotecaDeEstados;

    [Header("Configuração de Dano")]
    [SerializeField] private float duracaoEstadoDano = 0.15f;
    [SerializeField] private bool flashCorNoDano = true;
    [SerializeField] private Color corFlashDano = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Animações PrimeTween")]
    [SerializeField] private bool ativarTweens = true;
    [Tooltip("Elevação sutil ao andar para evitar que os pés atravessem o chão.")]
    [SerializeField] private float offsetAlturaAndando = 0.04f;
    [SerializeField] private float alturaPassoAndando = 0.1f;
    [SerializeField] private float tempoPassoAndando = 0.2f;

    [Header("Forças de Impacto")]
    [SerializeField] private Vector3 forcaShakeDano = new Vector3(0.16f, 0.04f, 0f);
    [SerializeField] private Vector3 forcaEscalaDano = new Vector3(0.08f, -0.06f, 0f);
    [SerializeField] private float intensidadeTremorAgarrado = 0.045f;

    private Dictionary<EstadoInimigo, GrupoSpritesInimigo> mapaEstados;
    private EstadoInimigo estadoBase = EstadoInimigo.Andando;
    private EstadoInimigo estadoVisualAtual = EstadoInimigo.Andando;

    private float tempoDanoRestante;
    private int frameAtual;
    private float timerAnimacao;

    private Transform alvoVisual;
    private Vector3 posicaoBase;
    private Vector3 escalaBase;
    private Color corBase = Color.white;

    private Tween tweenPosicao;
    private Tween tweenEscala;
    private Tween tweenCor;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            alvoVisual = spriteRenderer.transform;
            posicaoBase = alvoVisual.localPosition;
            escalaBase = alvoVisual.localScale;
            corBase = spriteRenderer.color;

            if (alvoVisual == transform)
            {
                Debug.LogWarning($"[EnemyVisualController | {gameObject.name}] Coloque o SpriteRenderer em um objeto filho (Visual2D) para os tweens não afetarem o movimento do inimigo.");
            }
        }

        mapaEstados = new Dictionary<EstadoInimigo, GrupoSpritesInimigo>();
        if (bibliotecaDeEstados != null)
        {
            foreach (var grupo in bibliotecaDeEstados)
            {
                if (!mapaEstados.ContainsKey(grupo.estado))
                {
                    mapaEstados.Add(grupo.estado, grupo);
                }
            }
        }
    }

    private void Start()
    {
        AplicarEstadoVisual(EstadoInimigo.Andando, true);
    }

    private void Update()
    {
        // Controla o retorno automático após o flash/pose de dano
        if (tempoDanoRestante > 0f && estadoBase != EstadoInimigo.Morto && estadoBase != EstadoInimigo.Agarrado)
        {
            tempoDanoRestante -= Time.deltaTime;
            if (tempoDanoRestante <= 0f)
            {
                AplicarEstadoVisual(estadoBase, true);
            }
        }

        AtualizarAnimacao();
    }

    public void DefinirEstadoBase(EstadoInimigo novoEstado)
    {
        if (estadoBase == novoEstado) return;

        estadoBase = novoEstado;

        // Se morreu ou foi agarrado, cancela o timer de dano imediatamente
        if (novoEstado == EstadoInimigo.Morto || novoEstado == EstadoInimigo.Agarrado)
        {
            tempoDanoRestante = 0f;
            AplicarEstadoVisual(novoEstado, true);
            return;
        }

        // Só troca o visual agora se não estiver no meio da reação de dano
        if (tempoDanoRestante <= 0f)
        {
            AplicarEstadoVisual(novoEstado, true);
        }
    }

    public void DispararDano()
    {
        if (estadoBase == EstadoInimigo.Morto || estadoBase == EstadoInimigo.Agarrado) return;

        tempoDanoRestante = duracaoEstadoDano;
        AplicarEstadoVisual(EstadoInimigo.LevandoDano, true);
    }

    private void AplicarEstadoVisual(EstadoInimigo estado, bool reiniciarFrames)
    {
        if (estadoVisualAtual == estado && !reiniciarFrames) return;

        estadoVisualAtual = estado;

        if (reiniciarFrames)
        {
            frameAtual = 0;
            timerAnimacao = 0f;
        }

        if (mapaEstados != null && mapaEstados.TryGetValue(estado, out GrupoSpritesInimigo grupo))
        {
            if (grupo.frames != null && grupo.frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = grupo.frames[0];
            }
        }

        AplicarTweenEstado(estado);
    }

    private void AplicarTweenEstado(EstadoInimigo estado)
    {
        if (!ativarTweens || alvoVisual == null) return;

        ResetarTransformECor();

        switch (estado)
        {
            case EstadoInimigo.Andando:
                float yBaseAndando = posicaoBase.y + offsetAlturaAndando;
                alvoVisual.localPosition = new Vector3(posicaoBase.x, yBaseAndando, posicaoBase.z);

                tweenPosicao = Tween.LocalPositionY(
                    alvoVisual,
                    yBaseAndando + alturaPassoAndando,
                    tempoPassoAndando,
                    Ease.OutQuad,
                    cycles: -1,
                    cycleMode: CycleMode.Yoyo
                );

                tweenEscala = Tween.Scale(
                    alvoVisual,
                    new Vector3(escalaBase.x * 1.03f, escalaBase.y * 0.97f, escalaBase.z),
                    tempoPassoAndando,
                    Ease.InOutSine,
                    cycles: -1,
                    cycleMode: CycleMode.Yoyo
                );
                break;

            case EstadoInimigo.Atacando:
                // Antecipação rápida (agacha) seguida de golpe expansivo
                tweenEscala = Tween.Scale(
                    alvoVisual,
                    new Vector3(escalaBase.x * 1.12f, escalaBase.y * 0.88f, escalaBase.z),
                    0.08f,
                    Ease.OutQuad
                ).OnComplete(() =>
                {
                    if (alvoVisual == null || estadoVisualAtual != EstadoInimigo.Atacando) return;
                    alvoVisual.localScale = escalaBase;
                    tweenEscala = Tween.PunchScale(alvoVisual, new Vector3(0.16f, 0.16f, 0f), 0.24f, frequency: 10);
                    tweenPosicao = Tween.PunchLocalPosition(alvoVisual, new Vector3(0f, 0.14f, 0f), 0.24f, frequency: 10);
                });
                break;

            case EstadoInimigo.LevandoDano:
                // Tremor brusco + squash de impacto + flash de cor a cada tiro acertado
                tweenPosicao = Tween.ShakeLocalPosition(alvoVisual, forcaShakeDano, duracaoEstadoDano, frequency: 24);
                tweenEscala = Tween.PunchScale(alvoVisual, forcaEscalaDano, duracaoEstadoDano, frequency: 20);

                if (flashCorNoDano && spriteRenderer != null)
                {
                    spriteRenderer.color = corFlashDano;
                    tweenCor = Tween.Color(spriteRenderer, corBase, duracaoEstadoDano, Ease.OutQuad);
                }
                break;

            case EstadoInimigo.Agarrado:
                // Vibração contínua de luta enquanto estiver preso/agarrado
                alvoVisual.localPosition = new Vector3(posicaoBase.x - intensidadeTremorAgarrado, posicaoBase.y, posicaoBase.z);
                tweenPosicao = Tween.LocalPositionX(
                    alvoVisual,
                    posicaoBase.x + intensidadeTremorAgarrado,
                    0.045f,
                    Ease.InOutSine,
                    cycles: -1,
                    cycleMode: CycleMode.Yoyo
                );

                tweenEscala = Tween.Scale(
                    alvoVisual,
                    new Vector3(escalaBase.x * 1.05f, escalaBase.y * 0.94f, escalaBase.z),
                    0.15f,
                    Ease.OutQuad
                );
                break;

            case EstadoInimigo.Morto:
                // Tranco final de queda e acomodação no chão
                tweenPosicao = Tween.PunchLocalPosition(alvoVisual, new Vector3(0f, 0.15f, 0f), 0.25f, frequency: 10)
                    .OnComplete(() =>
                    {
                        if (alvoVisual != null)
                        {
                            tweenPosicao = Tween.LocalPositionY(alvoVisual, posicaoBase.y - 0.04f, 0.15f, Ease.OutQuad);
                        }
                    });

                tweenEscala = Tween.PunchScale(alvoVisual, new Vector3(0.12f, -0.12f, 0f), 0.28f, frequency: 10);
                break;
        }
    }

    private void AtualizarAnimacao()
    {
        if (mapaEstados == null || !mapaEstados.TryGetValue(estadoVisualAtual, out GrupoSpritesInimigo grupo))
            return;

        if (grupo.frames == null || grupo.frames.Length <= 1 || spriteRenderer == null)
            return;

        float intervalo = 1f / Mathf.Max(grupo.fps, 0.1f);
        timerAnimacao += Time.deltaTime;

        if (timerAnimacao >= intervalo)
        {
            timerAnimacao -= intervalo;
            frameAtual++;

            if (frameAtual >= grupo.frames.Length)
            {
                // Estado Morto nunca faz loop, congela no corpo caído
                bool deveRepetir = grupo.loop && estadoVisualAtual != EstadoInimigo.Morto;
                frameAtual = deveRepetir ? 0 : grupo.frames.Length - 1;
            }

            spriteRenderer.sprite = grupo.frames[frameAtual];
        }
    }

    private void ResetarTransformECor()
    {
        PararTweens();

        if (alvoVisual != null)
        {
            alvoVisual.localPosition = posicaoBase;
            alvoVisual.localScale = escalaBase;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = corBase;
        }
    }

    private void PararTweens()
    {
        if (tweenPosicao.isAlive) tweenPosicao.Stop();
        if (tweenEscala.isAlive) tweenEscala.Stop();
        if (tweenCor.isAlive) tweenCor.Stop();
    }

    private void OnDisable()
    {
        ResetarTransformECor();
    }
}