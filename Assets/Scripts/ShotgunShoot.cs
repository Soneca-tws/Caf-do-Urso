using UnityEngine;
using UnityEngine.InputSystem; 

public class ShotgunShoot : MonoBehaviour
{
    [Header("References")]
    public Transform cam; 
    // O attackPoint foi removido do Raycast pois tiros Hitscan devem sair do centro da câmera para serem precisos,
    // mas você usará um attackPoint no futuro para instanciar o fogo saindo do cano (Muzzle Flash).

    [Header("Settings")]
    public int totalShots = 30; // Antigo totalThrows
    public float shootCooldown = 0.8f;
    
    [Header("Shotgun Spread")]
    public int pelletsPerShot = 8; // Quantidade de fragmentos disparados por clique
    public float spreadAmount = 0.05f; // O quão abertos os tiros vão sair
    public float range = 50f; // Alcance máximo da escopeta
    public float damagePerPellet = 10f; // Dano de cada fragmento individual

    bool readyToShoot;

    private void Start()
    {
        readyToShoot = true;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && readyToShoot && totalShots > 0)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        totalShots--;

        // Loop que se repete pela quantidade de balas da escopeta (ex: 8 vezes)
        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Calcula uma direção aleatória somando a direção da câmera com um pequeno vetor de espalhamento
            Vector3 spread = cam.forward + new Vector3(
                Random.Range(-spreadAmount, spreadAmount),
                Random.Range(-spreadAmount, spreadAmount),
                Random.Range(-spreadAmount, spreadAmount)
            );

            // Normaliza a direção para que o cálculo de distância do tiro não seja distorcido
            Vector3 shootDirection = spread.normalized;

            // Dispara o raio (Raycast) invisível a partir da câmera
            if (Physics.Raycast(cam.position, shootDirection, out RaycastHit hit, range))
            {
                // Mostra no console o nome do que a escopeta acertou
                Debug.Log("Acertou: " + hit.collider.name);

                // É AQUI que o seu sistema de interface entrará no futuro! Exemplo:
                // IDamageable inimigo = hit.collider.GetComponent<IDamageable>();
                // if (inimigo != null) inimigo.TakeDamage(damagePerPellet);
            }
        }

        // implement shootCooldown
        Invoke(nameof(ResetShoot), shootCooldown);
    }

    private void ResetShoot()
    {
        readyToShoot = true; 
    }
}