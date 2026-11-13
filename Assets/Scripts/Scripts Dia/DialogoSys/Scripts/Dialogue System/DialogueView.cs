using UnityEngine;
using TMPro;

public class DialogueView : MonoBehaviour
{
    public static DialogueView Instancia; 

    [SerializeField] private DialogueRunner runner;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Configurações")]
    public bool manterAbertoAoFinal = true; 

    // Variável que guarda o rosto do NPC atual
    private SpriteRenderer spriteAtivoAtual;

    private string textoSalvo = "";
    private string autorSalvo = "";
    private bool aInspecionar = false;

    private void Awake()
    {
        Instancia = this;
    }

    public void DefinirPersonagemAtual(SpriteRenderer spriteDoNPC)
    {
        spriteAtivoAtual = spriteDoNPC;
    }

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

    private void HidePanel()
    {
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

                if (line.ehOBarista)
                {
                    authorText.color = Color.cyan; 
                }
                else
                {
                    authorText.color = Color.white; 
                }
            }
        }

        dialogueText.text = line.text;

        // Lógica de mudança de emoções
        if (line.expressao != null && spriteAtivoAtual != null)
        {
            spriteAtivoAtual.sprite = line.expressao;
        }
    }

    public void ForcarFechamento()
    {
        dialoguePanel.SetActive(false);
    }

    public void MostrarInspecao(string titulo, string descricao)
    {
        if (!aInspecionar)
        {
            autorSalvo = authorText.text; 
            textoSalvo = dialogueText.text; 
            aInspecionar = true;
        }

        authorText.text = titulo;
        dialogueText.text = descricao;
    }

    public void OcultarInspecao()
    {
        if (aInspecionar)
        {
            authorText.text = autorSalvo;
            dialogueText.text = textoSalvo;
            aInspecionar = false;
        }
    }
}