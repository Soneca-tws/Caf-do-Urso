using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic; 

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    
    [Header("Detection Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    public LayerMask whatIsObstacle; // NOVO: Layer para as paredes e obstáculos!

    public float health;

    [Header("State")]
    public bool isDead; 
    public bool isGrabbed; 

    [Header("Debug State")]
    public string currentState; 

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
    public int poolSize = 15; 
    private List<GameObject> projectilePool;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        projectilePool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectile);
            obj.SetActive(false); 
            projectilePool.Add(obj);
        }
    }

    private void Update()
    {
        if (isDead || isGrabbed) 
        {
            if (isDead) currentState = "Dead";
            return;
        }

        // 1. O "Radar" detecta se o player está na área
        bool inSightSphere = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        bool inAttackSphere = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // 2. Confirma se não tem uma parede no meio do caminho!
        bool canSeePlayer = HasLineOfSight();

        // Só considera que viu/pode atacar se estiver no raio E tiver linha de visão
        playerInSightRange = inSightSphere && canSeePlayer;
        playerInAttackRange = inAttackSphere && canSeePlayer;

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
    // LINHA DE VISÃO (RAYCAST)
    // ==========================================
    private bool HasLineOfSight()
    {
        // Origem do olhar (na altura do peito/olhos do inimigo para não bater no chão)
        Vector3 origin = transform.position + new Vector3(0f, 1.5f, 0f);
        
        // Alvo (Peito do player)
        Vector3 target = player.position + new Vector3(0f, 1.2f, 0f);
        
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        // Dispara o raio verificando APENAS a layer das paredes
        // Se bater em uma parede antes da distância do player, ele não está vendo
        if (Physics.Raycast(origin, direction, distance, whatIsObstacle))
        {
            // Debug visual opcional para você ver o raio bloqueado na Unity
            Debug.DrawRay(origin, direction * distance, Color.red);
            return false;
        }

        // Debug visual do raio conectando a visão (verde)
        Debug.DrawRay(origin, direction * distance, Color.green);
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || isGrabbed) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement pm = collision.gameObject.GetComponent<PlayerMovement>();
            
            if (pm != null && pm.state == PlayerMovement.MovementState.dashing)
            {
                pm.FreezePlayerForGrab(1f); 
                StartGrab();
            }
        }
    }

    private void StartGrab()
    {
        isGrabbed = true;
        currentState = "Grabbed";

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        transform.SetParent(player);
        transform.localPosition = new Vector3(0f, 0f, 1.5f); 
        transform.localRotation = Quaternion.Euler(0, 180, 0); 
        Invoke(nameof(FinishGrabAndDie), 1f);
    }

    private void FinishGrabAndDie()
    {
        transform.SetParent(null);
        Die();
    }

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
            
            bullet.transform.position = transform.position + new Vector3(0f, 1.5f, 0f);
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