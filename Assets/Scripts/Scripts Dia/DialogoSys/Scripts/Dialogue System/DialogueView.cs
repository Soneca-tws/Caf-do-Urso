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
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning($"[DialogueView] Existe mais de um DialogueView na cena! ({Instancia.gameObject.name} e {gameObject.name})");
        }

        Instancia = this;

        if (runner == null)
        {
            runner = FindFirstObjectByType<DialogueRunner>();
        }
    }

    public void DefinirPersonagemAtual(NPCVisualController novoNPC)
    {
        npcVisualAtual = novoNPC;
        Debug.Log($"[DialogueView] Personagem atual definido para: {(novoNPC != null ? novoNPC.gameObject.name : "NULL")}");
    }

    private void OnEnable()
    {
        if (runner != null)
        {
            runner.OnDialogueStarted += ShowPanel;
            runner.OnLineStarted += DisplayLine;
            runner.OnDialogueEnded += HidePanel;
        }
    }

    private void OnDisable()
    {
        if (runner != null)
        {
            runner.OnDialogueStarted -= ShowPanel;
            runner.OnLineStarted -= DisplayLine;
            runner.OnDialogueEnded -= HidePanel;
        }
    }

    private void ShowPanel()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }

    private void HidePanel()
    {
        if (!manterAbertoAoFinal && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void DisplayLine(DialogueLine line)
    {
        if (npcVisualAtual == null || !npcVisualAtual.gameObject.activeInHierarchy)
        {
            npcVisualAtual = FindFirstObjectByType<NPCVisualController>();
        }

        bool mostrarAutor = !line.ehNarracao && !string.IsNullOrWhiteSpace(line.author);

        if (authorText != null)
        {
            authorText.gameObject.SetActive(mostrarAutor);

            if (mostrarAutor)
            {
                authorText.text = line.author;
                authorText.color = line.ehOBarista ? Color.cyan : Color.white;
            }
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.text;
            dialogueText.fontStyle = (line.ehNarracao && italicoNaNarracao) ? FontStyles.Italic : FontStyles.Normal;
        }

        if (npcVisualAtual != null)
        {
            Debug.Log($"[DialogueView] Fala de '{line.author}' -> Enviando estado '{line.estadoDoNPC}' para '{npcVisualAtual.gameObject.name}'");
            npcVisualAtual.MudarEstado(line.estadoDoNPC);
            npcVisualAtual.DefinirPausaFala(line.ehOBarista || line.ehNarracao);
        }
        else
        {
            Debug.LogWarning("[DialogueView] Nenhum NPCVisualController ativo foi encontrado na cena!");
        }
    }

    public void ForcarFechamento()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
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