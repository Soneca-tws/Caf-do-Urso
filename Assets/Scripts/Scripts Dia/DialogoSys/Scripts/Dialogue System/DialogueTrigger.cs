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
            // Esconde o botão de iniciar para o jogador não clicar de novo
            startButton.gameObject.SetActive(false);
            
            // Inicia o diálogo
            runner.StartDialogue(sequenceToPlay);
        });
    }

    // A função agora é pública e não acontece mais automaticamente.
    // Você vai chamá-la no futuro, quando a câmera voltar para o balcão.
    public void MostrarBotao()
    {
        startButton.gameObject.SetActive(true);
    }
}