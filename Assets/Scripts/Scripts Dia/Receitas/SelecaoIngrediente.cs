using UnityEngine;
using PrimeTween;

public class SelecaoIngrediente : MonoBehaviour
{
    [Header("Configurações")]
    public string nomeIngrediente;
    public bool isBase;

    [Header("Feedback de Seleção")]
    public float alturaSelecionado = 0.4f;
    [SerializeField] private float duracaoMovimento = 0.22f;

    [Header("Inspeção")]
    [TextArea(2, 4)]
    public string descricaoIngrediente;

    private bool selecionado = false;
    private Vector3 posicaoOriginal;
    private Tween tweenPosicao;

    private void Start()
    {
        posicaoOriginal = transform.localPosition;
    }

    private void OnMouseDown()
    {
        ToggleSelecao();
    }

    private void ToggleSelecao()
    {
        if (!selecionado)
        {
            if (!ReceitaManager.Instancia.TentarAdicionar(nomeIngrediente, this))
                return;

            selecionado = true;

            if (tweenPosicao.isAlive) tweenPosicao.Stop();
            tweenPosicao = Tween.LocalPositionY(
                transform,
                posicaoOriginal.y + alturaSelecionado,
                duracaoMovimento,
                Ease.OutBack
            );
        }
        else
        {
            ReceitaManager.Instancia.Remover(nomeIngrediente, this);
            ResetarVisual();
        }
    }

    // Chamado ao clicar novamente para desmarcar ou quando o ReceitaManager finaliza o pedido
    public void ResetarVisual()
    {
        selecionado = false;

        if (tweenPosicao.isAlive) tweenPosicao.Stop();
        tweenPosicao = Tween.LocalPosition(
            transform,
            posicaoOriginal,
            duracaoMovimento,
            Ease.OutQuad
        );
    }

    private void OnDisable()
    {
        if (tweenPosicao.isAlive) tweenPosicao.Stop();
    }

    private void OnMouseEnter()
    {
        if (DialogueView.Instancia != null)
        {
            DialogueView.Instancia.MostrarInspecao(nomeIngrediente, descricaoIngrediente);
        }
    }

    private void OnMouseExit()
    {
        if (DialogueView.Instancia != null)
        {
            DialogueView.Instancia.OcultarInspecao();
        }
    }
}