using UnityEngine;

public class SelecaoIngrediente : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeIngrediente;
    public bool isBase; 

    [Header("Visual 3D")]
    public Color corNormal = Color.white;
    public Color corSelecionado = Color.green; // Cor para mostrar que o cubo está ativo
    public float alturaSelecionado = 0.5f; 

    private bool selecionado = false;
    private Vector3 posicaoOriginal;
    private MeshRenderer meshRenderer; // Atualizado para ler a malha do Cubo 3D

    private void Start()
    {
        posicaoOriginal = transform.localPosition;
        
        // Pega o componente 3D do cubo
        meshRenderer = GetComponent<MeshRenderer>(); 
        
        // Define a cor inicial do material
        if (meshRenderer != null) 
            meshRenderer.material.color = corNormal;
    }

    private void OnMouseDown()
    {
        ToggleSelecao();
    }

   private void ToggleSelecao()
    {
        if (!selecionado)
        {
            // Envia o 'this' (este script) para o gerente saber quem está pedindo espaço
            if (!ReceitaManager.Instancia.TentarAdicionar(nomeIngrediente, this)) 
                return; 
            
            selecionado = true;
            if (meshRenderer != null) meshRenderer.material.color = corSelecionado;
            transform.localPosition = posicaoOriginal + new Vector3(0, alturaSelecionado, 0);
        }
        else
        {
            ReceitaManager.Instancia.Remover(nomeIngrediente, this);
            ResetarVisual(); // Usa a nova função para evitar código repetido
        }
    }

    // Função que o ReceitaManager vai chamar quando o café ficar pronto
    public void ResetarVisual()
    {
        selecionado = false;
        if (meshRenderer != null) meshRenderer.material.color = corNormal;
        transform.localPosition = posicaoOriginal;
    }
}