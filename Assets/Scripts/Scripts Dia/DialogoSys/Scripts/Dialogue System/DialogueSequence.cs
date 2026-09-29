using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string author;
    public bool ehOBarista; // A caixinha do Pedro fica aqui!
    [TextArea(3, 10)] public string text;
    public string[] tags; 
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Informações Básicas")]
    public string sequenceID;
    public List<DialogueLine> lines;

    [Header("Conexão (Opcional)")]
    [Tooltip("Arraste outro diálogo aqui para tocar logo após este terminar.")]
    public DialogueSequence proximoDialogo;

    [Header("Pedido do Cliente (Opcional)")]
    public List<string> receitaDesejada; 
    
    [Header("Feedback de Preparo")]
    public DialogueSequence dialogoSucesso; 
    public DialogueSequence dialogoErro;    
}