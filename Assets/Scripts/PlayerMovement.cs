using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed; 
    public float walkSpeed = 7f; 
    public float dashSpeed = 12f;
    public float groundDrag = 5f;

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

    private bool attemptingSlideJump = false;

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

    // ==========================================
    // ESTADO DE AGARRÃO / EXECUÇÃO
    // ==========================================
    public bool isGrabbing; // Trava o jogador no lugar

    public MovementState state;

    public enum MovementState
    {
        walking,
        dashing,
        sliding, 
        air,
        grabbing // Novo estado para pausar o jogador
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        readyToJump = true;
        startYScale = transform.localScale.y; 
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler(); 

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
        // Se estiver executando um inimigo, ignora TODOS os botões e zera a intenção de movimento
        if (isGrabbing)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            return;
        }

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
            
            if (state == MovementState.sliding)
            {
                attemptingSlideJump = true;
            }

            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); 
            currentSlideSpeed = slideBurstSpeed;
        }

        if (Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        // Força o estado de grabbing e zera a velocidade alvo
        if (isGrabbing)
        {
            state = MovementState.grabbing;
            moveSpeed = 0f;
            return;
        }

        bool isDashing = false;
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
            currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, crouchSpeed, slideDeceleration * Time.deltaTime);
            moveSpeed = currentSlideSpeed;
        }
        else if (grounded && isDashing)
        {
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        // Se estiver agarrando, não aplica nenhuma força
        if (isGrabbing) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
               rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded) 
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            if (attemptingSlideJump)
            {
                rb.AddForce(moveDirection.normalized * currentSlideSpeed * 10f * airMultiplier, ForceMode.Force);
                attemptingSlideJump = false;
            }
            else
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if(OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            if (attemptingSlideJump) return;

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

        if (state == MovementState.sliding)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.linearVelocity = flatVel.normalized * (currentSlideSpeed * 1.25f) + new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.AddForce(transform.up * (jumpForce * 1.2f), ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    // ==========================================
    // FUNÇÕES PARA CONTROLAR O CONGELAMENTO
    // ==========================================
    public void FreezePlayerForGrab(float duration)
    {
        isGrabbing = true;
        // Breca o personagem na hora, ignorando inércia!
        rb.linearVelocity = Vector3.zero; 
        
        // Descongela automaticamente após o tempo
        Invoke(nameof(UnfreezePlayer), duration);
    }

    private void UnfreezePlayer()
    {
        isGrabbing = false;
    }
    // ==========================================

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