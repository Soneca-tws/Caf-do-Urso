using UnityEngine;
using PrimeTween;

public class CoffeeCollectible : MonoBehaviour
{
    [Header("Animação Retrô (PrimeTween)")]
    [Tooltip("Graus por segundo no eixo vertical.")]
    public float rotationSpeed = 90f;
    [Tooltip("Velocidade da flutuação vertical.")]
    public float floatSpeed = 2f;
    [Tooltip("Altura máxima que o item sobe a partir da posição inicial.")]
    public float floatHeight = 0.2f;

    [Header("Configurações do Item")]
    public int coffeeAmount = 1; // Quantas sacas isso vale?
    public GameObject collectEffect;

    private Vector3 startLocalPos;
    private Quaternion startLocalRotation;

    private Tween tweenRotacao;
    private Tween tweenFlutuacao;

    private void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        // Garante que o item volte à posição e inclinação originais se o mapa for resetado
        transform.localPosition = startLocalPos;
        transform.localRotation = startLocalRotation;

        IniciarAnimacoes();
    }

    private void IniciarAnimacoes()
    {
        PararAnimacoes();

        // 1. Giro contínuo de 360° em torno do eixo vertical (World Y), ignorando a inclinação do pivô local
        if (Mathf.Abs(rotationSpeed) > 0.01f)
        {
            float duracaoGiro = 360f / Mathf.Abs(rotationSpeed);
            float anguloFinal = rotationSpeed >= 0f ? 360f : -360f;

            tweenRotacao = Tween.Custom(
                startValue: 0f,
                endValue: anguloFinal,
                duration: duracaoGiro,
                onValueChange: angulo =>
                {
                    transform.localRotation = Quaternion.AngleAxis(angle: angulo, axis: Vector3.up) * startLocalRotation;
                },
                ease: Ease.Linear,
                cycles: -1,
                cycleMode: CycleMode.Restart
            );
        }

        // 2. Flutuação suave para cima e de volta à altura inicial (sem afundar no chão)
        if (floatHeight > 0f && floatSpeed > 0.01f)
        {
            float duracaoSubida = Mathf.PI / floatSpeed;

            tweenFlutuacao = Tween.LocalPositionY(
                target: transform,
                endValue: startLocalPos.y + floatHeight,
                duration: duracaoSubida,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );
        }
    }

    private void PararAnimacoes()
    {
        if (tweenRotacao.isAlive) tweenRotacao.Stop();
        if (tweenFlutuacao.isAlive) tweenFlutuacao.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo encostou no café: " + other.gameObject.name + " | Tag atual: " + other.tag);

        if (other.CompareTag("Player"))
        {
            ColetarCafe();
        }
    }

    private void ColetarCafe()
    {
        Debug.Log("Saca de café coletada! Estoque + " + coffeeAmount);

        // PlayerInventory inventario = FindFirstObjectByType<PlayerInventory>();
        // inventario.AdicionarCafe(coffeeAmount);

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Desativa a saca do mapa (o OnDisable para os tweens automaticamente)
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        PararAnimacoes();
    }
}