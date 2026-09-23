using UnityEngine;
using TMPro;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private void OnEnable()
    {
        runner.OnDialogueStarted += ShowPanel;
        runner.OnLineStarted += DisplayLine;
        runner.OnDialogueEnded += HidePanel;
    }

    private void OnDisable()
    {
        runner.OnDialogueStarted -= ShowPanel;
        runner.OnLineStarted -= DisplayLine;
        runner.OnDialogueEnded -= HidePanel;
    }

    private void ShowPanel()
    {
        dialoguePanel.SetActive(true);
    }

    [Header("Configurações")]
    public bool manterAbertoAoFinal = true; // NOVO: Controla se o painel deve sumir ou ficar

    private void HidePanel()
    {
        // Só esconde o painel se a caixinha NÃO estiver marcada
        if (!manterAbertoAoFinal)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        bool hasAuthor = !string.IsNullOrWhiteSpace(line.author);

        if (authorText != null)
        {

            authorText.gameObject.SetActive(hasAuthor);

            if (hasAuthor)
            {
                authorText.text = line.author;
            }
        }

        dialogueText.text = line.text;
    }
}