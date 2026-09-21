using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string author;
    [TextArea(3, 10)] public string text;
    public string[] tags; // mudar sprites, tocar som e essas parada
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public string sequenceID;
    public List<DialogueLine> lines;
}