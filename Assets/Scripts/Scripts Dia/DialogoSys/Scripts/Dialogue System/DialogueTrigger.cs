using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Diálogos deste Cliente (Ordem dos Dias / Encontros)")]
    [Tooltip("Índice 0 = 1º encontro, Índice 1 = 2º encontro, e assim por diante.")]
    [SerializeField] private List<DialogueSequence> dialogos = new List<DialogueSequence>();

    private DialogueRunner runner;
    private NPCVisualController visualDesteNPC;
    private int indiceDialogoAtual = 0;

    private void Awake()
    {
        visualDesteNPC = GetComponentInChildren<NPCVisualController>();
        runner = FindFirstObjectByType<DialogueRunner>();
    }

    // Permite avançar qual diálogo esse cliente vai falar no próximo dia
    public void DefinirDialogoAtual(int indice)
    {
        if (dialogos == null || dialogos.Count == 0) return;
        indiceDialogoAtual = Mathf.Clamp(indice, 0, dialogos.Count - 1);
    }

    // Chamado automaticamente pelo ClienteMovimento ao chegar no balcão
    public void IniciarConversa()
    {
        if (dialogos == null || dialogos.Count == 0)
        {
            Debug.LogWarning($"DialogueTrigger em {gameObject.name} está com a lista de diálogos vazia!");
            return;
        }

        int indiceSeguro = Mathf.Clamp(indiceDialogoAtual, 0, dialogos.Count - 1);
        IniciarConversa(dialogos[indiceSeguro]);
    }

    // Sobrecarga caso um Gerenciador queira forçar um diálogo específico
    public void IniciarConversa(DialogueSequence sequenciaEspecifica)
    {
        if (sequenciaEspecifica == null) return;

        if (ReceitaManager.Instancia != null)
        {
            ReceitaManager.Instancia.DefinirPedidoAtual(sequenciaEspecifica);
        }

        if (DialogueView.Instancia != null && visualDesteNPC != null)
        {
            DialogueView.Instancia.DefinirPersonagemAtual(visualDesteNPC);
        }

        if (runner != null)
        {
            runner.StartDialogue(sequenciaEspecifica);
        }
    }
}