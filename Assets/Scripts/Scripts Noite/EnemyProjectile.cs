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
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("1. A bala reconheceu a tag Player!");
            
            PlayerSaude playerHealth = other.GetComponentInParent<PlayerSaude>();
            
            if (playerHealth != null) 
            {
                Debug.Log("2. A bala achou o script PlayerSaude!");
                playerHealth.Morrer();
            }
            else
            {
                Debug.LogError("ERRO: A bala bateu no Player, mas NÃO achou o script PlayerSaude nele!");
            }
        }

        Deactivate();
    }

    // Função que "devolve" a bala para a piscina (Pool)
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}