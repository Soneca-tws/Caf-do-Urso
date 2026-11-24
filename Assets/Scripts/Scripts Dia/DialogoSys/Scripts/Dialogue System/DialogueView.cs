using UnityEngine;
using TMPro;
using PrimeTween;

public class DialogueView : MonoBehaviour
{
    public static DialogueView Instancia;

    [SerializeField] private DialogueRunner runner;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI authorText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Configurações Gerais")]
    public bool manterAbertoAoFinal = true;
    public bool italicoNaNarracao = true;

    [Header("Animações PrimeTween")]
    [Tooltip("Velocidade da máquina de escrever em caracteres por segundo.")]
    [SerializeField] private float caracteresPorSegundo = 45f;
    [SerializeField] private float duracaoAbrirPainel = 0.28f;
    [SerializeField] private float duracaoFecharPainel = 0.2f;
    [SerializeField] private float forcaPulsoFala = 0.025f;

    private NPCVisualController npcVisualAtual;
    private CanvasGroup canvasGroupPainel;
    private Vector3 escalaOriginalPainel = Vector3.one;
    private Vector3 escalaOriginalAutor = Vector3.one;

    private Tween tweenPainelEscala;
    private Tween tweenPainelAlpha;
    private Tween tweenDigitacao;
    private Tween tweenAutor;

    private string textoSalvo = "";
    private string autorSalvo = "";
    private bool autorAtivoSalvo = true;
    private string ultimoAutorExibido = "";
    private bool aInspecionar = false;

    public bool EstaDigitando => tweenDigitacao.isAlive;

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

        if (authorText != null)
        {
            escalaOriginalAutor = authorText.transform.localScale;
        }

        if (dialoguePanel != null)
        {
            escalaOriginalPainel = dialoguePanel.transform.localScale;
            canvasGroupPainel = dialoguePanel.GetComponent<CanvasGroup>();
            if (canvasGroupPainel == null)
            {
                canvasGroupPainel = dialoguePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    public void DefinirPersonagemAtual(NPCVisualController novoNPC)
    {
        npcVisualAtual = novoNPC;
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

        PararTodosTweens();
    }

    private void ShowPanel()
    {
        if (dialoguePanel == null) return;

        PararTweensPainel();

        if (dialoguePanel.activeSelf && canvasGroupPainel.alpha >= 0.95f)
        {
            dialoguePanel.transform.localScale = escalaOriginalPainel;
            canvasGroupPainel.alpha = 1f;
            return;
        }

        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = escalaOriginalPainel * 0.92f;
        canvasGroupPainel.alpha = 0f;

        tweenPainelEscala = Tween.Scale(
            dialoguePanel.transform,
            escalaOriginalPainel,
            duracaoAbrirPainel,
            Ease.OutBack
        );

        tweenPainelAlpha = Tween.Alpha(
            canvasGroupPainel,
            1f,
            duracaoAbrirPainel * 0.8f,
            Ease.OutQuad
        );
    }

    private void HidePanel()
    {
        if (!manterAbertoAoFinal)
        {
            AnimarFechamentoPainel();
        }
    }

    public void ForcarFechamento()
    {
        AnimarFechamentoPainel();
    }

    private void AnimarFechamentoPainel()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        PararTweensPainel();
        if (tweenDigitacao.isAlive) tweenDigitacao.Stop();

        tweenPainelEscala = Tween.Scale(
            dialoguePanel.transform,
            escalaOriginalPainel * 0.94f,
            duracaoFecharPainel,
            Ease.InQuad
        );

        tweenPainelAlpha = Tween.Alpha(
            canvasGroupPainel,
            0f,
            duracaoFecharPainel,
            Ease.InQuad
        ).OnComplete(() =>
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
                dialoguePanel.transform.localScale = escalaOriginalPainel;
            }
        });
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

                if (line.author != ultimoAutorExibido)
                {
                    if (tweenAutor.isAlive) tweenAutor.Stop();
                    authorText.transform.localScale = escalaOriginalAutor;

                    Vector3 forcaProporcional = new Vector3(
                        escalaOriginalAutor.x * 0.1f,
                        escalaOriginalAutor.y * 0.1f,
                        0f
                    );
                    tweenAutor = Tween.PunchScale(authorText.transform, forcaProporcional, 0.22f);
                }

                ultimoAutorExibido = line.author;
            }
            else
            {
                ultimoAutorExibido = "";
            }
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.text;
            dialogueText.fontStyle = (line.ehNarracao && italicoNaNarracao) ? FontStyles.Italic : FontStyles.Normal;

            IniciarEfeitoDigitacao();
        }

        if (dialoguePanel != null && dialoguePanel.activeSelf && !tweenPainelEscala.isAlive)
        {
            dialoguePanel.transform.localScale = escalaOriginalPainel;
            Tween.PunchScale(dialoguePanel.transform, new Vector3(forcaPulsoFala, forcaPulsoFala, 0f), 0.18f);
        }

        if (npcVisualAtual != null)
        {
            npcVisualAtual.MudarEstado(line.estadoDoNPC);
            npcVisualAtual.DefinirPausaFala(line.ehOBarista || line.ehNarracao);
        }
    }

    private void IniciarEfeitoDigitacao()
    {
        if (tweenDigitacao.isAlive) tweenDigitacao.Stop();

        dialogueText.ForceMeshUpdate();
        int totalCaracteres = dialogueText.textInfo.characterCount;

        if (totalCaracteres <= 0 || caracteresPorSegundo <= 0f)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
            return;
        }

        dialogueText.maxVisibleCharacters = 0;
        float duracao = totalCaracteres / caracteresPorSegundo;

        tweenDigitacao = Tween.Custom(
            0f,
            totalCaracteres,
            duracao,
            valor =>
            {
                if (dialogueText != null)
                {
                    dialogueText.maxVisibleCharacters = Mathf.RoundToInt(valor);
                }
            },
            Ease.Linear
        ).OnComplete(() =>
        {
            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }
        });
    }

    public bool TentarCompletarDigitacao()
    {
        if (!tweenDigitacao.isAlive) return false;

        tweenDigitacao.Stop();
        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters = int.MaxValue;
        }
        return true;
    }

    public void MostrarInspecao(string titulo, string descricao)
    {
        if (tweenDigitacao.isAlive) tweenDigitacao.Stop();

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
            if (tweenAutor.isAlive) tweenAutor.Stop();
            authorText.transform.localScale = escalaOriginalAutor;
            authorText.gameObject.SetActive(true);
            authorText.text = titulo;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
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

            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.text = textoSalvo;
            aInspecionar = false;
        }
    }

    private void PararTweensPainel()
    {
        if (tweenPainelEscala.isAlive) tweenPainelEscala.Stop();
        if (tweenPainelAlpha.isAlive) tweenPainelAlpha.Stop();
    }

    private void PararTodosTweens()
    {
        PararTweensPainel();
        if (tweenDigitacao.isAlive) tweenDigitacao.Stop();
        if (tweenAutor.isAlive) tweenAutor.Stop();
    }
}