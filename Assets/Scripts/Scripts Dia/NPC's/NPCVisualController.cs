using System.Collections.Generic;
using UnityEngine;

public enum EstadoNPC
{
    ManterAtual,    // Não altera o estado atual (ideal para falas do Barista)
    Falando,
    Andando,
    Feliz,
    Triste,
    Raiva,
    Experimentando
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

    [Header("Configuração de Animação")]
    [Tooltip("Velocidade da troca de frames quando o estado tiver mais de 1 sprite.")]
    [SerializeField] private float fpsAnimacao = 6f;

    [Header("Biblioteca de Estados do NPC")]
    [SerializeField] private EstadoNPC estadoInicial = EstadoNPC.Andando;
    [SerializeField] private List<SpritesPorEstado> bibliotecaDeEstados = new List<SpritesPorEstado>();

    private EstadoNPC estadoAtual;
    private List<Sprite> spritesAtuais;
    private int frameAtual = 0;
    private float cronometroAnimacao = 0f;
    private bool animacaoPausada = false;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
                }
            }
        }
    }

    public void MudarEstado(EstadoNPC novoEstado)
    {
        Debug.Log($"[NPCVisualController | {gameObject.name}] MudarEstado chamado: Atual='{estadoAtual}' -> Novo='{novoEstado}'");

        if (novoEstado == EstadoNPC.ManterAtual)
        {
            Debug.Log($"[NPCVisualController | {gameObject.name}] Ignorado (ManterAtual).");
            return;
        }

        if (estadoAtual == novoEstado && spritesAtuais != null && spritesAtuais.Count > 0)
        {
            animacaoPausada = false;
            Debug.Log($"[NPCVisualController | {gameObject.name}] Já está em '{novoEstado}'. Mantendo sprites atuais.");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[NPCVisualController | {gameObject.name}] SpriteRenderer não atribuído!");
            return;
        }

        if (TentarAplicarEstado(novoEstado)) return;

        Debug.LogWarning($"[NPCVisualController | {gameObject.name}] Falha ao aplicar '{novoEstado}'. Tentando fallback para Falando...");

        if (novoEstado != EstadoNPC.Falando)
        {
            TentarAplicarEstado(EstadoNPC.Falando);
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

                    Debug.Log($"[NPCVisualController | {gameObject.name}] SUCESSO! Estado='{estadoAlvo}' | Sprite='{spritesAtuais[0].name}' aplicado em '{spriteRenderer.gameObject.name}'.");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[NPCVisualController | {gameObject.name}] Estado '{estadoAlvo}' está na biblioteca, mas a lista de sprites está vazia ou nula.");
                    return false;
                }
            }
        }

        Debug.LogWarning($"[NPCVisualController | {gameObject.name}] Estado '{estadoAlvo}' NÃO foi encontrado na Biblioteca De Estados.");
        return false;
    }

    public void DefinirPausaFala(bool pausar)
    {
        if (estadoAtual != EstadoNPC.Falando) return;

        animacaoPausada = pausar;

        if (pausar && spriteRenderer != null && spritesAtuais != null && spritesAtuais.Count > 0)
        {
            frameAtual = 0;
            spriteRenderer.sprite = spritesAtuais[0];
        }
    }

    public EstadoNPC ObterEstadoAtual()
    {
        return estadoAtual;
    }
}