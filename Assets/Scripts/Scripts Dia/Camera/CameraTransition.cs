using UnityEngine;
using System.Collections;

public class CameraTransition : MonoBehaviour
{
    [Header("Configurações da Câmera")]
    public Transform cameraPrincipal; 
    
    [Header("Pontos de Visão (Âncoras)")]
    public Transform visaoBalcao;
    public Transform visaoPrateleira;
    
    [Header("Velocidade")]
    public float velocidadeTransicao = 3f;

    private bool olhandoPrateleira = false;
    private Coroutine transicaoAtual;

    
    public void AlternarVisao()
    {
        olhandoPrateleira = !olhandoPrateleira;

        
        if (transicaoAtual != null)
        {
            StopCoroutine(transicaoAtual);
        }

        // Decide a ancora
        Transform alvo = olhandoPrateleira ? visaoPrateleira : visaoBalcao;
        transicaoAtual = StartCoroutine(MoverCamera(alvo));
    }

    private IEnumerator MoverCamera(Transform alvo)
    {
        // Enquanto a câmera não chegar muito perto do alvo, ela continua movendo
        while (Vector3.Distance(cameraPrincipal.position, alvo.position) > 0.01f || 
               Quaternion.Angle(cameraPrincipal.rotation, alvo.rotation) > 0.1f)
        {
            // O Lerp faz o movimento suave (suavizando a posição e a rotação)
            cameraPrincipal.position = Vector3.Lerp(cameraPrincipal.position, alvo.position, Time.deltaTime * velocidadeTransicao);
            cameraPrincipal.rotation = Quaternion.Lerp(cameraPrincipal.rotation, alvo.rotation, Time.deltaTime * velocidadeTransicao);
            
            yield return null; 
        }

        // Garante que cravou exatamente na posição final
        cameraPrincipal.position = alvo.position;
        cameraPrincipal.rotation = alvo.rotation;
    }
}