using UnityEngine;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    [SerializeField] private DialogueSequence sequenceToPlay;
    [SerializeField] private Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(() => runner.StartDialogue(sequenceToPlay));
    }
}