using System;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public enum EstadoNPC
{
    ManterAtual,    // Não altera o estado atual (ideal para falas do Barista)
    Falando,
    Andando,
    Feliz,
    Triste,
    Raiva,
    Experimentando,
    Extra1,
    Extra2,
    Extra3
}

[System.Serializable]
public struct SpritesPorEstado
{
    public EstadoNPC estado;
    [Tooltip("Coloque 1 sprite para expressão fixa ou 2+ sprites para animar em loop.")]
    public List<Sprite> sprites;
}

public class NPCVisualController : MonoBehaviour
{
    [Header("Referência Visual")]
    [Tooltip("Arraste o filho Visual2D (com SpriteRenderer) deste NPC aqui.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Configuração de Frames")]
    [Tooltip("Velocidade da troca de frames quando o estado tiver mais de 1 sprite.")]
    [SerializeField] private float fpsAnimacao = 6f;

    [Header("Animações PrimeTween")]
    [SerializeField] private bool ativarAnimacoesTween = true;
    [Tooltip("Elevação extra aplicada apenas enquanto anda para evitar que os pés entrem no chão.")]
    [SerializeField] private float offsetAlturaAndando = 0.08f;
    [SerializeField] private float alturaPassoAndando = 0.12f;
    [SerializeField] private float inclinacaoAndando = 3f;
    [SerializeField] private float tempoPassoAndando = 0.22f;

    [Header("Impacto na Troca de Emoção e Frames")]
    [SerializeField] private bool impactoAoTrocarEmocao = true;
    [SerializeField] private bool impactoAoTrocarFrame = true;

    [Tooltip("Força do tremor/pulso nas emoções normais (Falando, Triste, Experimentando, etc.).")]
    [SerializeField] private Vector3 forcaShakeNormal = new Vector3(0.055f, 0.018f, 0f);
    [SerializeField] private Vector3 forcaEscalaNormal = new Vector3(0.035f, -0.025f, 0f);
    [SerializeField] private float duracaoImpactoNormal = 0.18f;

    [Tooltip("Força do tremor/pulso quando a emoção for Raiva.")]
    [SerializeField] private Vector3 forcaShakeRaiva = new Vector3(0.17f, 0.05f, 0f);
    [SerializeField] private Vector3 forcaEscalaRaiva = new Vector3(0.09f, -0.06f, 0f);
    [SerializeField] private float duracaoImpactoRaiva = 0.34f;

    [Header("Biblioteca de Estados do NPC")]
    [SerializeField] private EstadoNPC estadoInicial = EstadoNPC.Andando;
    [SerializeField] private List<SpritesPorEstado> bibliotecaDeEstados = new List<SpritesPorEstado>();

    private EstadoNPC estadoAtual;
    private List<Sprite> spritesAtuais;
    private int frameAtual = 0;
    private float cronometroAnimacao = 0f;
    private bool animacaoPausada = false;
    private bool executandoImpactoEmocao = false;

    private Transform alvoVisual;
    private Vector3 posicaoBaseVisual;
    private Vector3 escalaBaseVisual;
    private Vector3 rotacaoBaseVisual;

    private Tween tweenPosicao;
    private Tween tweenEscala;
    private Tween tweenRotacao;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            alvoVisual = spriteRenderer.transform;
            posicaoBaseVisual = alvoVisual.localPosition;
            escalaBaseVisual = alvoVisual.localScale;
            rotacaoBaseVisual = alvoVisual.localEulerAngles;
        }
    }

    private void Start()
    {
        MudarEstado(estadoInicial);
    }

    private void Update()
    {
        if (spritesAtuais != null && spritesAtuais.Count > 1 && !animacaoPausada)
        {
            cronometroAnimacao += Time.deltaTime;
            float intervalo = 1f / Mathf.Max(0.1f, fpsAnimacao);

            if (cronometroAnimacao >= intervalo)
            {
                cronometroAnimacao -= intervalo;
                frameAtual = (frameAtual + 1) % spritesAtuais.Count;

                if (spriteRenderer != null && spritesAtuais[frameAtual] != null)
                {
                    spriteRenderer.sprite = spritesAtuais[frameAtual];

                    if (impactoAoTrocarFrame && !executandoImpactoEmocao)
                    {
                        DispararImpacto(estadoAtual == EstadoNPC.Raiva);
                    }
                }
            }
        }
    }

    public void MudarEstado(EstadoNPC novoEstado)
    {
        if (novoEstado == EstadoNPC.ManterAtual) return;

        if (estadoAtual == novoEstado && spritesAtuais != null && spritesAtuais.Count > 0)
        {
            animacaoPausada = false;
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[NPCVisualController | {gameObject.name}] SpriteRenderer não atribuído!");
            return;
        }

        if (TentarAplicarEstado(novoEstado))
        {
            AplicarTweenDoEstado(novoEstado);
            return;
        }

        if (novoEstado != EstadoNPC.Falando && TentarAplicarEstado(EstadoNPC.Falando))
        {
            AplicarTweenDoEstado(EstadoNPC.Falando);
        }
    }

    private bool TentarAplicarEstado(EstadoNPC estadoAlvo)
    {
        for (int i = 0; i < bibliotecaDeEstados.Count; i++)
        {
            if (bibliotecaDeEstados[i].estado == estadoAlvo)
            {
                List<Sprite> lista = bibliotecaDeEstados[i].sprites;

                if (lista != null && lista.Count > 0 && lista[0] != null)
                {
                    estadoAtual = estadoAlvo;
                    spritesAtuais = lista;
                    frameAtual = 0;
                    cronometroAnimacao = 0f;
                    animacaoPausada = false;
                    spriteRenderer.sprite = spritesAtuais[0];
                    return true;
                }
            }
        }

        return false;
    }

    private void AplicarTweenDoEstado(EstadoNPC estado)
    {
        if (!ativarAnimacoesTween || alvoVisual == null) return;

        ResetarTransformVisual();

        if (estado == EstadoNPC.Andando)
        {
            float yBaseAndando = posicaoBaseVisual.y + offsetAlturaAndando;
            alvoVisual.localPosition = new Vector3(posicaoBaseVisual.x, yBaseAndando, posicaoBaseVisual.z);

            tweenPosicao = Tween.LocalPositionY(
                alvoVisual,
                yBaseAndando + alturaPassoAndando,
                tempoPassoAndando,
                Ease.OutQuad,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );

            alvoVisual.localEulerAngles = rotacaoBaseVisual + new Vector3(0f, 0f, -inclinacaoAndando);
            tweenRotacao = Tween.LocalRotation(
                alvoVisual,
                rotacaoBaseVisual + new Vector3(0f, 0f, inclinacaoAndando),
                tempoPassoAndando * 2f,
                Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );
            return;
        }

        if (estado == EstadoNPC.Feliz)
        {
            executandoImpactoEmocao = true;
            tweenPosicao = Tween.PunchLocalPosition(alvoVisual, new Vector3(0f, 0.22f, 0f), 0.38f, frequency: 8);
            tweenEscala = Tween.PunchScale(alvoVisual, new Vector3(0.08f, 0.08f, 0f), 0.35f, frequency: 8)
                .OnComplete(() =>
                {
                    executandoImpactoEmocao = false;
                    IniciarAnimacaoContinuaDoEstado(EstadoNPC.Feliz);
                });
            return;
        }

        bool ehRaiva = estado == EstadoNPC.Raiva;

        if (impactoAoTrocarEmocao)
        {
            executandoImpactoEmocao = true;
            DispararImpacto(ehRaiva, () =>
            {
                executandoImpactoEmocao = false;
                IniciarAnimacaoContinuaDoEstado(estado);
            });
        }
        else
        {
            IniciarAnimacaoContinuaDoEstado(estado);
        }
    }

    private void IniciarAnimacaoContinuaDoEstado(EstadoNPC estado)
    {
        if (!ativarAnimacoesTween || alvoVisual == null || estadoAtual != estado) return;

        switch (estado)
        {
            case EstadoNPC.Triste:
                tweenPosicao = Tween.LocalPositionY(alvoVisual, posicaoBaseVisual.y - 0.06f, 0.3f, Ease.OutQuad);
                tweenEscala = Tween.Scale(
                    alvoVisual,
                    new Vector3(escalaBaseVisual.x * 1.02f, escalaBaseVisual.y * 0.94f, escalaBaseVisual.z),
                    0.3f,
                    Ease.OutQuad
                );
                break;

            case EstadoNPC.Experimentando:
                IniciarRespiracaoSuave(0.45f, 1.03f);
                break;

            case EstadoNPC.Raiva:
                break;

            default:
                IniciarRespiracaoSuave(1.1f, 1.02f);
                break;
        }
    }

    private void DispararImpacto(bool ehRaiva, Action aoTerminar = null)
    {
        if (!ativarAnimacoesTween || alvoVisual == null || estadoAtual == EstadoNPC.Andando) return;

        if (tweenPosicao.isAlive) tweenPosicao.Stop();
        if (tweenEscala.isAlive) tweenEscala.Stop();

        float offsetY = (estadoAtual == EstadoNPC.Triste && !executandoImpactoEmocao) ? -0.06f : 0f;
        Vector3 escalaReferencia = (estadoAtual == EstadoNPC.Triste && !executandoImpactoEmocao)
            ? new Vector3(escalaBaseVisual.x * 1.02f, escalaBaseVisual.y * 0.94f, escalaBaseVisual.z)
            : escalaBaseVisual;

        alvoVisual.localPosition = posicaoBaseVisual + new Vector3(0f, offsetY, 0f);
        alvoVisual.localScale = escalaReferencia;

        Vector3 forcaShake = ehRaiva ? forcaShakeRaiva : forcaShakeNormal;
        Vector3 forcaEscala = ehRaiva ? forcaEscalaRaiva : forcaEscalaNormal;
        float duracao = ehRaiva ? duracaoImpactoRaiva : duracaoImpactoNormal;
        int frequencia = ehRaiva ? 22 : 14;

        tweenPosicao = Tween.ShakeLocalPosition(alvoVisual, forcaShake, duracao, frequency: frequencia);
        tweenEscala = Tween.PunchScale(alvoVisual, forcaEscala, duracao * 0.9f, frequency: frequencia)
            .OnComplete(() => aoTerminar?.Invoke());
    }

    private void IniciarRespiracaoSuave(float duracao, float multiplicadorY)
    {
        if (!ativarAnimacoesTween || alvoVisual == null) return;

        if (tweenEscala.isAlive) tweenEscala.Stop();
        alvoVisual.localScale = escalaBaseVisual;

        tweenEscala = Tween.Scale(
            alvoVisual,
            new Vector3(escalaBaseVisual.x, escalaBaseVisual.y * multiplicadorY, escalaBaseVisual.z),
            duracao,
            Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo
        );
    }

    public void DefinirPausaFala(bool pausar)
    {
        if (estadoAtual != EstadoNPC.Falando) return;

        animacaoPausada = pausar;

        if (pausar)
        {
            if (spriteRenderer != null && spritesAtuais != null && spritesAtuais.Count > 0)
            {
                frameAtual = 0;
                spriteRenderer.sprite = spritesAtuais[0];
            }

            if (!executandoImpactoEmocao)
            {
                IniciarRespiracaoSuave(1.1f, 1.02f);
            }
        }
        else if (ativarAnimacoesTween && alvoVisual != null && !executandoImpactoEmocao && !tweenPosicao.isAlive)
        {
            alvoVisual.localPosition = posicaoBaseVisual;
            tweenPosicao = Tween.PunchLocalPosition(alvoVisual, new Vector3(0f, 0.07f, 0f), 0.2f, frequency: 10);
        }
    }

    private void ResetarTransformVisual()
    {
        executandoImpactoEmocao = false;
        PararTweens();

        if (alvoVisual != null)
        {
            alvoVisual.localPosition = posicaoBaseVisual;
            alvoVisual.localScale = escalaBaseVisual;
            alvoVisual.localEulerAngles = rotacaoBaseVisual;
        }
    }

    private void PararTweens()
    {
        if (tweenPosicao.isAlive) tweenPosicao.Stop();
        if (tweenEscala.isAlive) tweenEscala.Stop();
        if (tweenRotacao.isAlive) tweenRotacao.Stop();
    }

    private void OnDisable()
    {
        ResetarTransformVisual();
    }

    public EstadoNPC ObterEstadoAtual()
    {
        return estadoAtual;
    }
}