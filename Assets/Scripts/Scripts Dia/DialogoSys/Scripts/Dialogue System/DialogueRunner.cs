using System;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    public event Action OnDialogueStarted;
    public event Action<DialogueLine> OnLineStarted;
    public event Action OnDialogueEnded;

    private DialogueSequence currentSequence;
    private int currentIndex;
    private bool isPlaying;
    private bool isSingleLineMode;

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentIndex = 0;
        isPlaying = true;
        isSingleLineMode = false;

        OnDialogueStarted?.Invoke();
        PlayNextLine();
    }


    public void PlayRandomLine(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
        {
            Debug.LogWarning("[DialogueRunner] SEQUENCIA VAZIA OU ERRO SLA.");
            return;
        }

        currentSequence = sequence;
        currentIndex = UnityEngine.Random.Range(0, sequence.lines.Count);
        isPlaying = true;
        isSingleLineMode = true;

        OnDialogueStarted?.Invoke();
        OnLineStarted?.Invoke(currentSequence.lines[currentIndex]);
    }

    public void AdvanceDialogue()
    {
        if (!isPlaying) return;

        if (isSingleLineMode)
        {
            EndDialogue();
            return;
        }

        currentIndex++;
        PlayNextLine();
    }

    private void PlayNextLine()
    {
        if (currentIndex < currentSequence.lines.Count)
        {
            OnLineStarted?.Invoke(currentSequence.lines[currentIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void StopDialogue()
    {
        if (isPlaying)
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        // Guarda a referência do próximo diálogo antes de limpar a sequência atual
        DialogueSequence proximoNaFila = currentSequence != null ? currentSequence.proximoDialogo : null;

        isPlaying = false;
        currentSequence = null;
        isSingleLineMode = false;

        // Se houver um diálogo pendurado, inicia a próxima conversa automaticamente!
        if (proximoNaFila != null)
        {
            StartDialogue(proximoNaFila);
        }
        else
        {
            // SÓ AVISA o resto do jogo que acabou se não houver mais nada na fila!
            // É isso que vai impedir a câmara de fugir para a prateleira antes do tempo.
            OnDialogueEnded?.Invoke();
        }
    }
}