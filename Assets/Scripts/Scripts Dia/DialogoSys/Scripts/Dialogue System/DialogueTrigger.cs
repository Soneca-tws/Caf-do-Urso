using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueSequence sequenceToPlay;

    [Header("Visual do NPC")]
    [Tooltip("Arraste o componente NPCVisualController deste personagem aqui.")]
    public NPCVisualController visualDesteNPC;

    public void IniciarConversa()
    {
        if (ReceitaManager.Instancia != null)
        {
            ReceitaManager.Instancia.DefinirPedidoAtual(sequenceToPlay);
        }

        if (DialogueView.Instancia != null && visualDesteNPC != null)
        {
            DialogueView.Instancia.DefinirPersonagemAtual(visualDesteNPC);
        }

        if (runner != null && sequenceToPlay != null)
        {
            runner.StartDialogue(sequenceToPlay);
        }
    }
}