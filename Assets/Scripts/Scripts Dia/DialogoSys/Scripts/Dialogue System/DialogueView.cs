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
    public bool italicoNaNarracao = true;

    private NPCVisualController npcVisualAtual;

    private string textoSalvo = "";
    private string autorSalvo = "";
    private bool autorAtivoSalvo = true;
    private bool aInspecionar = false;

    private void Awake()
    {
        Instancia = this;
    }

    public void DefinirPersonagemAtual(NPCVisualController novoNPC)
    {
        npcVisualAtual = novoNPC;
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
        bool mostrarAutor = !line.ehNarracao && !string.IsNullOrWhiteSpace(line.author);

        if (authorText != null)
        {
            authorText.gameObject.SetActive(mostrarAutor);

            if (mostrarAutor)
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

        if (dialogueText != null)
        {
            dialogueText.text = line.text;
            dialogueText.fontStyle = (line.ehNarracao && italicoNaNarracao) ? FontStyles.Italic : FontStyles.Normal;
        }

        if (npcVisualAtual != null)
        {
            npcVisualAtual.MudarEstado(line.estadoDoNPC);
            npcVisualAtual.DefinirPausaFala(line.ehOBarista || line.ehNarracao);
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
            if (authorText != null)
            {
                autorSalvo = authorText.text;
                autorAtivoSalvo = authorText.gameObject.activeSelf;
            }
            textoSalvo = dialogueText.text;
            aInspecionar = true;
        }

        if (authorText != null)
        {
            authorText.gameObject.SetActive(true);
            authorText.text = titulo;
        }

        dialogueText.text = descricao;
    }

    public void OcultarInspecao()
    {
        if (aInspecionar)
        {
            if (authorText != null)
            {
                authorText.gameObject.SetActive(autorAtivoSalvo);
                authorText.text = autorSalvo;
            }

            dialogueText.text = textoSalvo;
            aInspecionar = false;
        }
    }
}