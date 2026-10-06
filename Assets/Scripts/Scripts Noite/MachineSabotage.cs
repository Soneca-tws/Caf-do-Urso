using UnityEngine;

public class MachineSabotage : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Interface")]
    public GameObject telaDeVitoria; // NOVO: Arraste o seu PainelVitoria para cá na Unity

    [Header("Debug State")]
    public string currentState;

    [Header("Visuals (Optional)")]
    public Renderer machineRenderer;
    public Material intactMaterial;
    public Material sabotagedMaterial;

    private bool isSabotaged = false;

    public enum MachineState
    {
        Intact,     
        Sabotaged,  
        Disabled    
    }

    public MachineState state;

    private void Start()
    {
        currentHealth = maxHealth;
        state = MachineState.Intact;
        currentState = "Intact";

        if (machineRenderer != null && intactMaterial != null)
        {
            machineRenderer.material = intactMaterial;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case MachineState.Intact:
                currentState = "Intact";
                break;

            case MachineState.Sabotaged:
                currentState = "Sabotaged";
                break;

            case MachineState.Disabled:
                currentState = "Disabled";
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isSabotaged) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("A máquina sofreu dano! Vida restante: " + currentHealth);

        if (currentHealth <= 0f)
        {
            SabotageMachine();
        }
    }

    private void SabotageMachine()
    {
        isSabotaged = true;
        state = MachineState.Sabotaged;

        if (machineRenderer != null && sabotagedMaterial != null)
        {
            machineRenderer.material = sabotagedMaterial;
        }

        Debug.Log("A máquina foi completamente sabotada!");

        // --- NOVO: Ativa a tela verde de Vitória ---
        if (telaDeVitoria != null)
        {
            telaDeVitoria.SetActive(true);
            
            // Destrava o mouse para clicar em possíveis botões
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Pausa o jogo
            Time.timeScale = 0f; 
        }
    }
}