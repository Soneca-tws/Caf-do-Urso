using UnityEngine;
using UnityEngine.SceneManagement; 

public class PlayerSaude : MonoBehaviour
{
    [Header("Interface")]
    public GameObject telaDeMorte; 

    public bool morto = false;

    // Transformamos em pública (public) para o projétil conseguir acessá-la
    public void Morrer()
    {
        Debug.Log("3. A função Morrer() começou a rodar!");
        if (morto) return;
        
        morto = true;
        
        if (telaDeMorte != null)
        {
            Debug.Log("4. Ligando o painel na tela!");
            telaDeMorte.SetActive(true);
        }
        else
        {
            Debug.LogError("ERRO GRAVE: O script tentou ligar a tela, mas o espaço 'Tela De Morte' no Inspector está vazio (None)!");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; 
    }

    public void RecomecarJogo()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}