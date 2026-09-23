using UnityEngine;
using System.Collections;

public class CameraTransition : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public Transform cameraPrincipal; 
    public Transform visaoBalcao;
    public Transform visaoPrateleira;
    public float velocidadeTransicao = 3f;

    [Header("Integração de Diálogo")]
    public DialogueRunner runner; // Arraste o seu DialogueManager aqui!

    private Coroutine transicaoAtual;

    // Se inscreve para escutar quando o diálogo acaba
    private void OnEnable()
    {
        if (runner != null)
        {
            runner.OnDialogueEnded += IrParaPrateleira;
        }
    }

    // Boa prática: sempre se desinscrever no OnDisable
    private void OnDisable()
    {
        if (runner != null)
        {
            runner.OnDialogueEnded -= IrParaPrateleira;
        }
    }

    // Agora são funções separadas para você poder voltar para o balcão depois!
    public void IrParaPrateleira()
    {
        if (transicaoAtual != null) StopCoroutine(transicaoAtual);
        transicaoAtual = StartCoroutine(MoverCamera(visaoPrateleira));
    }

    public void IrParaBalcao()
    {
        if (transicaoAtual != null) StopCoroutine(transicaoAtual);
        transicaoAtual = StartCoroutine(MoverCamera(visaoBalcao));
    }

    private IEnumerator MoverCamera(Transform alvo)
    {
        while (Vector3.Distance(cameraPrincipal.position, alvo.position) > 0.01f || 
               Quaternion.Angle(cameraPrincipal.rotation, alvo.rotation) > 0.1f)
        {
            cameraPrincipal.position = Vector3.Lerp(cameraPrincipal.position, alvo.position, Time.deltaTime * velocidadeTransicao);
            cameraPrincipal.rotation = Quaternion.Lerp(cameraPrincipal.rotation, alvo.rotation, Time.deltaTime * velocidadeTransicao);
            
            yield return null; 
        }

        cameraPrincipal.position = alvo.position;
        cameraPrincipal.rotation = alvo.rotation;
    }
}