using UnityEngine;
using UnityEngine.InputSystem; // 1. Importação do Novo Sistema de Inputs

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f; // Adicionado um valor padrão sugerido

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        MyInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        // 2. Verificação de segurança: garante que há um teclado conectado
        if (Keyboard.current == null) return;

        // Zera os inputs a cada frame antes de ler novamente
        horizontalInput = 0f;
        verticalInput = 0f;

        // 3. Leitura das teclas WASD 
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
    }

    private void MovePlayer()
    {
        // Calcula a direção do movimento baseada na orientação da câmera
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Aplica a força de movimento. O uso de .normalized evita que andar na diagonal seja mais rápido
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}