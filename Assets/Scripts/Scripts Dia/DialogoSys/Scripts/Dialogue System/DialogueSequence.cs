using System.Collections.Generic;
using UnityEngine;

// 1. ESTA É A ESTRUTURA QUE A UNITY DISSE QUE ESTAVA FALTANDO
[System.Serializable]
public struct DialogueLine
{
    public string author;
    [TextArea(3, 10)] public string text;
    public string[] tags; // mudar sprites, tocar som e essas parada
}

// 2. ESTE É O SCRIPTABLE OBJECT PRINCIPAL
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Informações Básicas")]
    public string sequenceID;
    public List<DialogueLine> lines; // O erro estava acontecendo nesta linha!

    [Header("Pedido do Cliente (Opcional)")]
    [Tooltip("Nomes exatos dos ingredientes que o cliente quer. Deixe vazio se for apenas um diálogo casual.")]
    public List<string> receitaDesejada; 
    
    [Header("Feedback de Preparo")]
    public DialogueSequence dialogoSucesso; // Disparado se a receita bater
    public DialogueSequence dialogoErro;    // Disparado se a receita estiver errada
}