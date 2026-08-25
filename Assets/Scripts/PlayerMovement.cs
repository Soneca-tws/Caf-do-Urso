using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f; 
    public float groundDrag = 5f; // Recomendado testar valores entre 5 e 7

    [Header("Ground Check")]
    public float playerHeight = 2f; // Altura padrão de uma cápsula na Unity
    public LayerMask whatIsGround;
    bool grounded;

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
        // Ground Check: Dispara um raio para baixo a partir do centro do jogador.
        // O cálculo 'playerHeight * 0.5f + 0.2f' vai do centro até o pé, adicionando 0.2f de margem.
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();

        // Controle de Atrito (Drag)
        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        // Verificação de segurança para o teclado
        if (Keyboard.current == null) return;

        // Reset dos inputs
        horizontalInput = 0f;
        verticalInput = 0f;

        // Captura das teclas WASD
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
    }

    private void MovePlayer()
    {
        // Calcula a direção baseada para onde a câmera/orientation está apontando
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Aplica a força contínua no Rigidbody
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}