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
    public DialogueRunner runner; 

    private Coroutine transicaoAtual;
    private bool travarMovimentoPrateleira = false; 

    private void OnEnable()
    {
        if (runner != null)
        {
            runner.OnDialogueEnded += IrParaPrateleira;
        }
    }

    private void OnDisable()
    {
        if (runner != null)
        {
            runner.OnDialogueEnded -= IrParaPrateleira;
        }
    }

    public void IrParaBalcao()
    {
        if (transicaoAtual != null) StopCoroutine(transicaoAtual);
        transicaoAtual = StartCoroutine(MoverCamera(visaoBalcao));
    }

    public void IrParaPrateleira()
    {
        // Se a trava estiver ativa, a câmera recusa a ordem
        if (travarMovimentoPrateleira) return; 

        if (transicaoAtual != null) StopCoroutine(transicaoAtual);
        transicaoAtual = StartCoroutine(MoverCamera(visaoPrateleira));
    }
    
    // Função pública para o ClienteMovimento ativar a trava
    public void FinalizarExpediente()
    {
        travarMovimentoPrateleira = true;
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