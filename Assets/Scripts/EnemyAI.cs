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
        // Se o inimigo estiver morto, atualiza o debug e encerra a execução do Update
        if (isDead) 
        {
            currentState = "Dead";
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

    // 2. FUNÇÃO PARA PEGAR UMA BALA DISPONÍVEL
    private GameObject GetProjectileFromPool()
    {
        for (int i = 0; i < projectilePool.Count; i++)
        {
            // TRAVA DE SEGURANÇA: Se a bala sumiu do jogo por algum motivo, pula para a próxima
            if (projectilePool[i] == null) continue;

            // Se achar uma bala que está desligada, entrega ela
            if (!projectilePool[i].activeInHierarchy)
            {
                return projectilePool[i];
            }
        }

        // Se o pool esvaziar, cria uma bala extra
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
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

        if (!alreadyAttacked)
        {
            // 3. ATIRA USANDO O POOL EM VEZ DO INSTANTIATE
            GameObject bullet = GetProjectileFromPool();
            
            // Reposiciona a bala na arma do inimigo
            bullet.transform.position = transform.position;
            bullet.transform.rotation = Quaternion.identity;
            bullet.SetActive(true); // Liga a bala

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            
            // IMPORTANTE: Como a bala é reciclada, precisamos zerar a velocidade do tiro anterior!
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;

            // CORREÇÃO DA MIRA: Calcula a direção do peito/centro do player com precisão
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
        // Se já estiver morto, não toma mais dano (evita rodar o código de morte duas vezes)
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

        // 1. Para o movimento do NavMeshAgent imediatamente
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 2. Desliga o Collider para o player não ficar tropeçando no corpo morto
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Destrói o objeto depois de 5 segundos para limpar a cena
        Invoke(nameof(DestroyEnemy), 5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject); 
    }
}