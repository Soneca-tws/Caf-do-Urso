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
            // Esconde o botão de iniciar para o jogador não clicar nele por acidente durante a fala
            startButton.gameObject.SetActive(false);
            
            // Inicia o diálogo
            runner.StartDialogue(sequenceToPlay);
        });

        // Inscreve uma função para quando o diálogo acabar
        runner.OnDialogueEnded += MostrarBotao;
    }

    private void MostrarBotao()
    {
        // Traz o botão de volta para a tela quando a conversa terminar
        startButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        // Limpeza de segurança (boa prática na Unity ao usar eventos)
        if (runner != null)
        {
            runner.OnDialogueEnded -= MostrarBotao;
        }
    }
}