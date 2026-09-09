using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed; // eu ACHO que ele é controlado pelo StateHandler agora
    public float walkSpeed = 7f; 
    public float dashSpeed = 12f;
    public float groundDrag = 5f;

    // Nova seção para as configurações do Slide com Desaceleração
    [Header("Sliding")]
    [Tooltip("A velocidade inicial máxima do slide (o impulso).")]
    public float slideBurstSpeed = 25f; 
    
    [Tooltip("A velocidade final quando o slide perde a força (agachado).")]
    public float crouchSpeed = 4f; 
    
    [Tooltip("O quão rápido ele freia. Mude esse valor no Inspector para ajustar a derrapagem!")]
    public float slideDeceleration = 35f; 
    
    private float currentSlideSpeed; 
    public float slideYScale = 0.5f; 
    private float startYScale; 

    [Header("Jumping")]
    public float jumpForce = 13f; 
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight = 2f; 
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb;

    // 1. Definição do StateMachine
    public MovementState state;

    public enum MovementState
    {
        walking,
        dashing,
        sliding, // Novo estado de movimento adicionado para o Slide
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        readyToJump = true;

        // Salva a altura inicial do player para podermos voltar a ela depois do Slide
        startYScale = transform.localScale.y; 
    }

    private void Update()
    {
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler(); // 2. Chama o StateHandler.

        // Controle de atrito
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
        if (Keyboard.current == null) return;

        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;

        if (Keyboard.current.spaceKey.isPressed && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Verifica se a tecla Ctrl foi pressionada neste exato frame para iniciar o Slide
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            // Reduz a escala Y do player para simular o agachamento/deslizamento
            transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
            // Aplica uma força para baixo para evitar que o player flutue ao reduzir de tamanho
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); 

            // Reseta a velocidade atual do slide para o impulso máximo!
            currentSlideSpeed = slideBurstSpeed;
        }

        // Verifica se a tecla Ctrl foi solta neste exato frame para finalizar o Slide
        if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
        {
            // Retorna o player ao tamanho original salvo no Start
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // 3. Verificação dos botões de Dash 
        bool isDashing = false;
        // Variável adicionada para checar se o Ctrl está sendo segurado
        bool isSliding = false; 
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.isPressed) isDashing = true;
            if (Keyboard.current.leftCtrlKey.isPressed) isSliding = true; 
        }
            
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            isDashing = true;

        if (grounded && isSliding)
        {
            state = MovementState.sliding;

            
            // MoveTowards vai diminuindo a currentSlideSpeed até chegar na crouchSpeed
            currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, crouchSpeed, slideDeceleration * Time.deltaTime);
            
            // Aplica a velocidade atualizada no player
            moveSpeed = currentSlideSpeed;
        }
        // Modo - Dashing 
        else if (grounded && isDashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }
        // Modo - Walking 
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        // Modo - Air 
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //calcula a direção de movimento
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //onslope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
               rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded) // O 'else if' adicionado aqui para consertar o bug do pulo na rampa não esquecer caso apareça dnv
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        //desliga a gravidade quando tá on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // limitador de velocidade on slope
        if(OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // limitador de velocidade no chão ou on air
        else
        {
             Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //limita a velicdade se necessário
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}