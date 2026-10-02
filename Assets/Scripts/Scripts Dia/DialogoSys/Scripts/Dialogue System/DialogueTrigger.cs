using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueSequence sequenceToPlay;

    // Função pública que qualquer outro script pode chamar para iniciar a conversa
    public void IniciarConversa()
    {
        // Envia o pedido para o Gerente de Receita
        if (ReceitaManager.Instancia != null)
        {
            ReceitaManager.Instancia.DefinirPedidoAtual(sequenceToPlay);
        }
        
        // Inicia o diálogo na tela
        if (runner != null && sequenceToPlay != null)
        {
            runner.StartDialogue(sequenceToPlay);
        }
    }
}