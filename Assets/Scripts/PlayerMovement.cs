using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f; 
    public float groundDrag = 5f;

    [Header("Ground Check")]
    public float playerHeight = 2f; 
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
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();

        // Controle dinâmico de atrito linear (Linear Damping)
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

    private void SpeedControl()
    {
        // Isola a velocidade nos eixos X e Z, ignorando o eixo Y (gravidade/pulo)
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Limita a velocidade caso ultrapasse o limite estabelecido (moveSpeed)
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            
            // Aplica a velocidade limitada mantendo a velocidade vertical original
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}