using UnityEngine;
using UnityEngine.InputSystem; // 1. Importação obrigatória para o Novo Sistema

public class PlayerCam : MonoBehaviour
{
    // pega o input do mouse
    public float sensX = 20f;
    public float sensy = 20f;

    public Transform orientation;

// Encontra a rotação atual da câmera
    float xRotation;
    float yRotation; 

    private void Start()
    {
        // Trava e oculta o cursor no centro da tela
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Verificação de segurança: garante que há um mouse conectado e ativo
        if (Mouse.current == null) return;

        //  Captura o movimento (delta) bruto do mouse no frame atual
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        //  Aplica a sensibilidade com um multiplicador de suavização (0.01f)
        float mouseX = mouseDelta.x * sensX * 0.01f;
        float mouseY = mouseDelta.y * sensy * 0.01f;

        // Calcula a rotação nos eixos X e Y
        yRotation += mouseX;
        xRotation -= mouseY;
        
        // Limita a rotação vertical para não virar de ponta-cabeça
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplica a rotação à câmera (olhar para cima/baixo e para os lados)
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        
        // Aplica a rotação ao orientation (apenas eixo Y horizontal)
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}