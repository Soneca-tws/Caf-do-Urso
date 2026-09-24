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
    public GameObject botaoPronto; // Arraste o botão da UI para cá

    [Header("Referências")]
    public CameraTransition transicaoCamera; 

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        // Garante que o botão comece desligado quando o jogo rodar
        AtualizarBotaoPronto();
    }

    public bool TentarAdicionar(string ingrediente, SelecaoIngrediente cubo)
    {
        if (ingredientesSelecionados.Count < limiteIngredientes)
        {
            ingredientesSelecionados.Add(ingrediente);
            cubosAtivos.Add(cubo); 
            Debug.Log($"[{ingredientesSelecionados.Count}/3] {ingrediente} adicionado.");
            
            AtualizarBotaoPronto(); // Checa se deve mostrar o botão
            return true;
        }
        
        Debug.Log("Limite atingido! Desmarque algo primeiro.");
        return false;
    }

    public void Remover(string ingrediente, SelecaoIngrediente cubo)
    {
        if (ingredientesSelecionados.Contains(ingrediente))
        {
            ingredientesSelecionados.Remove(ingrediente);
            cubosAtivos.Remove(cubo); 
            Debug.Log($"[{ingredientesSelecionados.Count}/3] {ingrediente} removido.");
            
            AtualizarBotaoPronto(); // Checa se deve esconder o botão
        }
    }

    public void PrepararBebida()
    {
        if (ingredientesSelecionados.Count == limiteIngredientes)
        {
            Debug.Log("Café finalizado com: " + string.Join(", ", ingredientesSelecionados));
            
            // Limpa o visual dos cubos
            foreach (SelecaoIngrediente cubo in cubosAtivos)
            {
                cubo.ResetarVisual();
            }
            
            // Zera a memória do gerente
            ingredientesSelecionados.Clear();
            cubosAtivos.Clear();

            // Esconde o botão para o próximo cliente
            AtualizarBotaoPronto();

            if (transicaoCamera != null)
            {
                transicaoCamera.IrParaBalcao();
            }
        }
    }

    // Função que liga ou desliga o botão baseado na quantidade
    private void AtualizarBotaoPronto()
    {
        if (botaoPronto != null)
        {
            // O botão só fica ativo (true) se a quantidade for exatamente 3
            bool prontoParaEntregar = ingredientesSelecionados.Count == limiteIngredientes;
            botaoPronto.SetActive(prontoParaEntregar);
        }
    }
}