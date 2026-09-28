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

    public void ForcarFechamento()
    {
        dialoguePanel.SetActive(false);
    }

    // Adicione isto no topo das variáveis da classe
    public static DialogueView Instancia; 

    private string textoSalvo = "";
    private string autorSalvo = "";
    private bool aInspecionar = false;

    // Se já tiver um Awake(), adicione apenas a linha Instancia = this; dentro dele
    private void Awake()
    {
        Instancia = this;
    }

    // Função para substituir o texto temporariamente
    public void MostrarInspecao(string titulo, string descricao)
    {
        if (!aInspecionar)
        {
            // Guarda o texto atual do cliente
            // ATENÇÃO: Substitua 'authorText' e 'dialogueText' pelos nomes exatos das suas variáveis de UI (TextMeshProUGUI)
            autorSalvo = authorText.text; 
            textoSalvo = dialogueText.text; 
            aInspecionar = true;
        }

        authorText.text = titulo;
        dialogueText.text = descricao;
    }

    // Função para repor o texto original
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