using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;
    public float lifeTime = 5f; // Tempo máximo voando antes de sumir

    // O OnEnable roda toda vez que a bala é retirada do Pool e ligada (SetActive(true))
    private void OnEnable()
    {
        // Por segurança, cancela qualquer contagem de tempo que tenha ficado da vez anterior
        CancelInvoke(nameof(Deactivate));
        
        // Inicia um cronômetro para desativar a bala se ela for atirada para o céu e não bater em nada
        Invoke(nameof(Deactivate), lifeTime);
    }

    // Usamos OnTriggerEnter pois a bala deve ter a caixa "Is Trigger" marcada no Collider
    private void OnTriggerEnter(Collider other)
    {
        // Se a bala bater no próprio inimigo que atirou (ou em outro inimigo), ela ignora e continua voando
        if (other.CompareTag("Enemy")) return;

        // Se bater no Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("O Player tomou um tiro!");
            
            // FUTURO: Aqui você chamará o script de vida do jogador
            // Exemplo:
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null) playerHealth.TakeDamage(damage);
        }

        // Se bater no Player, no chão, ou na parede, a bala se desliga para voltar ao Pool
        Deactivate();
    }

    // Função que "devolve" a bala para a piscina (Pool)
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}