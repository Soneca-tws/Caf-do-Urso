using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public float health;

    [Header("State")]
    public bool isDead; // Controla se o inimigo está vivo ou morto
    public bool isGrabbed; // NOVO: Controla se o inimigo está sendo segurado pelo jogador

    [Header("Debug State")]
    public string currentState; // Mostra o estado atual da IA diretamente no Inspector

    // Patrulha
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Ataque
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    // Estados
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // ==========================================
    // SISTEMA DE OBJECT POOLING
    // ==========================================
    [Header("Object Pooling")]
    public int poolSize = 15; // Quantidade de balas pré-carregadas dentro do pool
    private List<GameObject> projectilePool;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        // 1. INICIALIZA O POOL
        projectilePool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectile);
            obj.SetActive(false); // A bala nasce invisível/desligada
            projectilePool.Add(obj);
        }
    }

    private void Update()
    {
        // Se o inimigo estiver morto OU sendo agarrado, ele ignora a IA de combate
        if (isDead || isGrabbed) 
        {
            if (isDead) currentState = "Dead";
            return;
        }

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) 
        {
            currentState = "Patroling";
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange) 
        {
            currentState = "Chasing (Alert)";
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange) 
        {
            currentState = "Attacking";
            AttackPlayer();
        }
    }

    // ==========================================
    // NOVA MECÂNICA: AGARRAR (DASH KILL)
    // ==========================================
    private void OnCollisionEnter(Collision collision)
{
    if (isDead || isGrabbed) return;

    if (collision.gameObject.CompareTag("Player"))
    {
        PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
        
        if (pm != null && pm.state == PlayerMovement.MovementState.dashing)
        {
            // Congela o jogador por 1 segundo exato (mesmo tempo que o inimigo leva pra morrer)
            pm.FreezePlayerForGrab(1f); 
            
            StartGrab();
        }
    }
}

    private void StartGrab()
    {
        isGrabbed = true;
        currentState = "Grabbed";

        // 1. Para o agente de andar imediatamente
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 2. Desliga a colisão temporariamente para o corpo do inimigo não atrapalhar o movimento do player
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Gruda o inimigo no jogador!
        transform.SetParent(player);
        
        // Posição: Fica exatamente 1.5 metros na frente do jogador (ajuste esse Z conforme a grossura do seu inimigo)
        transform.localPosition = new Vector3(0f, 0f, 1.5f); 
        
        // Faz o inimigo olhar para a mesma direção que o jogador (de costas pro player) ou olhar pro player
        transform.localRotation = Quaternion.Euler(0, 180, 0); // Vira ele de frente para a câmera do jogador

        // 4. Espera 1 segundo de paralisia e então solta e mata o inimigo
        Invoke(nameof(FinishGrabAndDie), 1f);
    }

    private void FinishGrabAndDie()
    {
        // Desgruda o inimigo do jogador para o cadáver ficar no chão onde ele "morreu"
        transform.SetParent(null);
        
        // Opcional: religar o collider se quiser que o cadáver tenha física depois
        
        // Chama a morte normal do inimigo
        Die();
    }
    // ==========================================

    // 2. FUNÇÃO PARA PEGAR UMA BALA DISPONÍVEL
    private GameObject GetProjectileFromPool()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            if (projectilePool[i] == null) continue;

            if (!projectilePool[i].activeInHierarchy)
            {
                return projectilePool[i];
            }
        }

        GameObject newObj = Instantiate(projectile);
        newObj.SetActive(false);
        projectilePool.Add(newObj);
        return newObj;
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        if (!alreadyAttacked)
        {
            GameObject bullet = GetProjectileFromPool();
            
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;
            bullet.SetActive(true); 

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;

            Vector3 aimTarget = new Vector3(player.position.x, player.position.y + 1.2f, player.position.z);
            Vector3 aimDirection = (aimTarget - transform.position).normalized;

            rb.AddForce(aimDirection * 32f, ForceMode.Impulse);
            
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        
        if (health <= 0) 
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentState = "Dead";

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Invoke(nameof(DestroyEnemy), 5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject); 
    }
}