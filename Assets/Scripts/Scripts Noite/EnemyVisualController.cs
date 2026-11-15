using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<EstadoInimigo, GrupoSpritesInimigo> mapaEstados;
    private EstadoInimigo estadoBase = EstadoInimigo.Andando;
    private EstadoInimigo estadoVisualAtual = EstadoInimigo.Andando;

    private float tempoDanoRestante;
    private int frameAtual;
    private float timerAnimacao;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        mapaEstados = new Dictionary<EstadoInimigo, GrupoSpritesInimigo>();
        foreach (var grupo in bibliotecaDeEstados)
        {
            if (!mapaEstados.ContainsKey(grupo.estado))
            {
                mapaEstados.Add(grupo.estado, grupo);
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
}