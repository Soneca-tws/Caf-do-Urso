using System.Collections.Generic;
using UnityEngine;

public class ReceitaManager : MonoBehaviour
{
    public static ReceitaManager Instancia;
    
    [Header("Regras do Jogo")]
    public int limiteIngredientes = 3;
    public List<string> ingredientesSelecionados = new List<string>();
    private List<SelecaoIngrediente> cubosAtivos = new List<SelecaoIngrediente>();

    [Header("Interface")]
    public GameObject botaoPronto;

    [Header("Referências")]
    public CameraTransition transicaoCamera; 

    [Header("Sistema de Pedidos")]
    public DialogueRunner dialogueRunner; 
    public DialogueView interfaceDialogo; 
    private DialogueSequence pedidoAtual; 

    private bool ultimoPreparoFoiSucesso;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        AtualizarBotaoPronto();
    }

    public void DefinirPedidoAtual(DialogueSequence novoPedido)
    {
        pedidoAtual = novoPedido;
    }

    public bool TentarAdicionar(string ingrediente, SelecaoIngrediente cubo)
    {
        if (ingredientesSelecionados.Count < limiteIngredientes)
        {
            ingredientesSelecionados.Add(ingrediente);
            cubosAtivos.Add(cubo); 
            AtualizarBotaoPronto(); 
            return true;
        }
        return false;
    }

    public void Remover(string ingrediente, SelecaoIngrediente cubo)
    {
        if (ingredientesSelecionados.Contains(ingrediente))
        {
            ingredientesSelecionados.Remove(ingrediente);
            cubosAtivos.Remove(cubo); 
            AtualizarBotaoPronto(); 
        }
    }

    public void PrepararBebida()
    {
        if (ingredientesSelecionados.Count > 0)
        {
            ultimoPreparoFoiSucesso = ValidarReceita();

            // --- NOVO: AVISA O CLIENTE QUE ELE JÁ TEM O CAFÉ ---
            ClienteMovimento cliente = Object.FindFirstObjectByType<ClienteMovimento>();
            if (cliente != null)
            {
                cliente.ReceberCafe();
            }
            // ----------------------------------------------------

            foreach (SelecaoIngrediente cubo in cubosAtivos)
            {
                cubo.ResetarVisual();
            }
            
            ingredientesSelecionados.Clear();
            cubosAtivos.Clear();
            AtualizarBotaoPronto();

            if (interfaceDialogo != null) interfaceDialogo.ForcarFechamento();
            if (transicaoCamera != null) transicaoCamera.IrParaBalcao();

            if (pedidoAtual != null)
            {
                Invoke(nameof(TocarFeedback), 0.5f); 
            }
        }
    }

    private bool ValidarReceita()
    {
        if (pedidoAtual == null || pedidoAtual.receitaDesejada == null || pedidoAtual.receitaDesejada.Count == 0) 
            return true;

        Debug.Log("--- INICIANDO VALIDAÇÃO ---");
        
        // 1. Mostra o que o cliente pediu
        Debug.Log($"O cliente quer {pedidoAtual.receitaDesejada.Count} item(s):");
        foreach(string item in pedidoAtual.receitaDesejada) 
        { 
            Debug.Log($"-> Pedido: [{item}]"); 
        }

        // 2. Mostra o que está na xícara
        Debug.Log($"A xícara tem {ingredientesSelecionados.Count} item(s):");
        foreach(string item in ingredientesSelecionados) 
        { 
            Debug.Log($"-> Xícara: [{item}]"); 
        }

        // 3. Verifica a quantidade
        if (ingredientesSelecionados.Count != pedidoAtual.receitaDesejada.Count)
        {
            Debug.Log("Resultado: ERRO - Você colocou ingredientes a mais ou a menos.");
            return false;
        }

        // 4. Verifica os nomes
        foreach (string itemDesejado in pedidoAtual.receitaDesejada)
        {
            if (!ingredientesSelecionados.Contains(itemDesejado)) 
            {
                Debug.Log($"Resultado: ERRO - O item [{itemDesejado}] não foi reconhecido na xícara.");
                return false; 
            }
        }
        
        Debug.Log("Resultado: SUCESSO - A receita bateu 100%!");
        return true; 
    }

    private void TocarFeedback()
    {
        // Usa a anotação que fizemos lá em cima, em vez de validar a xícara vazia de novo
        DialogueSequence resposta = ultimoPreparoFoiSucesso ? pedidoAtual.dialogoSucesso : pedidoAtual.dialogoErro;
        
        if (resposta != null && resposta.lines != null && resposta.lines.Count > 0 && dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(resposta);
        }
        else
        {
            Debug.LogWarning("Aviso: O diálogo de resposta está vazio no Inspector!");
        }
    }

    // Deixamos apenas UMA versão desta função!
    private void AtualizarBotaoPronto()
    {
        if (botaoPronto != null)
        {
            // O botão fica ativo (true) se houver pelo menos 1 item selecionado
            bool prontoParaEntregar = ingredientesSelecionados.Count > 0;
            botaoPronto.SetActive(prontoParaEntregar);
        }
    }
}