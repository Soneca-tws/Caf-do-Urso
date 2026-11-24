using UnityEngine;
using PrimeTween;

public class EfeitoHover : MonoBehaviour
{
    [Header("Configurações do Hover")]
    [Tooltip("Tempo em segundos para completar uma volta de 360 graus.")]
    [SerializeField] private float duracaoGiro = 2.5f;
    [SerializeField] private float aumentoTamanho = 1.1f;
    [SerializeField] private float duracaoTransicao = 0.2f;

    private Vector3 escalaOriginal;
    private Quaternion rotacaoOriginal;

    private Tween tweenGiro;
    private Tween tweenEscala;
    private SelecaoIngrediente selecaoIngrediente;

    private void Start()
    {
        escalaOriginal = transform.localScale;
        rotacaoOriginal = transform.localRotation;
        selecaoIngrediente = GetComponent<SelecaoIngrediente>();
    }

    private void OnMouseEnter()
    {
        PararTweens();

        // Leve aumento de escala para destacar o foco do cursor
        tweenEscala = Tween.Scale(transform, escalaOriginal * aumentoTamanho, duracaoTransicao, Ease.OutBack);

        // Só gira se o item NÃO estiver marcado como Is Base
        bool ehBase = selecaoIngrediente != null && selecaoIngrediente.isBase;
        if (!ehBase)
        {
            Vector3 rotacaoAtual = transform.localEulerAngles;
            tweenGiro = Tween.LocalEulerAngles(
                transform,
                rotacaoAtual,
                rotacaoAtual + new Vector3(0f, 360f, 0f),
                duracaoGiro,
                Ease.Linear,
                cycles: -1,
                cycleMode: CycleMode.Restart
            );
        }
    }

    private void OnMouseExit()
    {
        PararTweens();

        tweenEscala = Tween.Scale(transform, escalaOriginal, duracaoTransicao, Ease.OutQuad);
        tweenGiro = Tween.LocalRotation(transform, rotacaoOriginal, duracaoTransicao, Ease.OutQuad);
    }

    private void PararTweens()
    {
        if (tweenGiro.isAlive) tweenGiro.Stop();
        if (tweenEscala.isAlive) tweenEscala.Stop();
    }

    private void OnDisable()
    {
        PararTweens();
    }
}