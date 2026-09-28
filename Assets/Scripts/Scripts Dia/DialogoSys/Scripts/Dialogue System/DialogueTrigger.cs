using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueSequence sequenceToPlay;
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(() => 
        {
            // Esconde o botão
            startButton.gameObject.SetActive(false);
            
            // ESSA É A LINHA CRÍTICA QUE DEVE ESTAR FALTANDO:
            // Ela envia o "TesteBalcao" para o Gerente de Receita ler o feedback
            if (ReceitaManager.Instancia != null)
            {
                ReceitaManager.Instancia.DefinirPedidoAtual(sequenceToPlay);
            }
            
            // Inicia o diálogo
            runner.StartDialogue(sequenceToPlay);
        });
    }

    public void MostrarBotao()
    {
        startButton.gameObject.SetActive(true);
    }
}