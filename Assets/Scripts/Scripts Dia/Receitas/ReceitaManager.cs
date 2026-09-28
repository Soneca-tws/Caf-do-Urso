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
    public DialogueView interfaceDialogo; // NOVO: Referência para limpar a tela
    private DialogueSequence pedidoAtual; 

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
        if (ingredientesSelecionados.Count == limiteIngredientes)
        {
            bool acertou = ValidarReceita();

            foreach (SelecaoIngrediente cubo in cubosAtivos)
            {
                cubo.ResetarVisual();
            }
            
            ingredientesSelecionados.Clear();
            cubosAtivos.Clear();
            AtualizarBotaoPronto();

            // NOVO: Apaga o diálogo antigo imediatamente
            if (interfaceDialogo != null)
            {
                interfaceDialogo.ForcarFechamento();
            }

            if (transicaoCamera != null)
            {
                transicaoCamera.IrParaBalcao();
            }

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

        foreach (string itemDesejado in pedidoAtual.receitaDesejada)
        {
            if (!ingredientesSelecionados.Contains(itemDesejado)) return false; 
        }
        
        return true; 
    }

    private void TocarFeedback()
    {
        DialogueSequence resposta = ValidarReceita() ? pedidoAtual.dialogoSucesso : pedidoAtual.dialogoErro;
        
        // NOVO: Proteção contra campos vazios no Inspector
        if (resposta != null && resposta.lines != null && resposta.lines.Count > 0 && dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(resposta);
        }
        else
        {
            Debug.LogWarning("Aviso: O diálogo de resposta está vazio no Inspector!");
        }
    }

    private void AtualizarBotaoPronto()
    {
        if (botaoPronto != null)
        {
            botaoPronto.SetActive(ingredientesSelecionados.Count == limiteIngredientes);
        }
    }
}