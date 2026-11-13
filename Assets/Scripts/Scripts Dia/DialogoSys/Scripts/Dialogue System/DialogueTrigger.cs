using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueSequence sequenceToPlay;

    [Header("Visual do NPC")]
    [Tooltip("Arraste o filho Visual2D deste NPC específico aqui!")]
    public SpriteRenderer spriteDesteNPC; // <-- Adicionamos isto!

    public void IniciarConversa()
    {
        // Envia o pedido para o Gerente de Receita
        if (ReceitaManager.Instancia != null)
        {
            ReceitaManager.Instancia.DefinirPedidoAtual(sequenceToPlay);
        }
        
        // A MÁGICA: Avisa a Interface de quem é o rosto que vai mudar agora!
        if (DialogueView.Instancia != null && spriteDesteNPC != null)
        {
            DialogueView.Instancia.DefinirPersonagemAtual(spriteDesteNPC);
        }
        
        // Inicia o diálogo na tela
        if (runner != null && sequenceToPlay != null)
        {
            runner.StartDialogue(sequenceToPlay);
        }
    }
}