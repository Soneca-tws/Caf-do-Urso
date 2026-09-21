using UnityEngine;

public class MachineSabotage : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Debug State")]
    public string currentState; // Mostra o estado atual da máquina no Inspector

    [Header("Visuals (Optional)")]
    public Renderer machineRenderer;
    public Material intactMaterial;
    public Material sabotagedMaterial;

    private bool isSabotaged = false;

    // Estados possíveis da máquina
    public enum MachineState
    {
        Intact,     // Funcionando normalmente
        Sabotaged,  // Vida chegou a zero / sabotada
        Disabled    // Desligada completamente
    }

    public MachineState state;

    private void Start()
    {
        currentHealth = maxHealth;
        state = MachineState.Intact;
        currentState = "Intact";

        // Define o visual inicial se houver um material configurado
        if (machineRenderer != null && intactMaterial != null)
        {
            machineRenderer.material = intactMaterial;
        }
    }

    private void Update()
    {
        // Máquina de Estados simples para a Máquina
        switch (state)
        {
            case MachineState.Intact:
                currentState = "Intact";
                // Lógica da máquina funcionando normalmente (se houver)
                break;

            case MachineState.Sabotaged:
                currentState = "Sabotaged";
                // Lógica da máquina sabotada (soltando fumaça, gerando alerta, etc.)
                break;

            case MachineState.Disabled:
                currentState = "Disabled";
                break;
        }
    }

    // Método chamado pelo tiro da escopeta (igualzinho ao TakeDamage do inimigo)
    public void TakeDamage(int damage)
    {
        // Se já foi sabotada, ignorar novos tiros
        if (isSabotaged) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("A máquina sofreu dano! Vida restante: " + currentHealth);

        // Se a vida acabou, ativa o estado de sabotagem
        if (currentHealth <= 0f)
        {
            SabotageMachine();
        }
    }

    private void SabotageMachine()
    {
        isSabotaged = true;
        state = MachineState.Sabotaged;

        // Muda a cor/material da máquina para indicar que foi sabotada
        if (machineRenderer != null && sabotagedMaterial != null)
        {
            machineRenderer.material = sabotagedMaterial;
        }

        Debug.Log("A máquina foi completamente sabotada!");

        // Aqui você pode disparar eventos do jogo, abrir portas, desativar alarmes, etc.
    }
}