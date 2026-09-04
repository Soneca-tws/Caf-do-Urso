using UnityEngine;
using UnityEngine.InputSystem; 
public class Shoot : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow; 

    [Header("Settings")]
    public int totalThrows; 
    public float throwCooldown;
    
    [Header("Throwing")]
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;

    private void Start()
    {
        readyToThrow = true;
    }

    private void Update()
    {
        // Verifica se há um mouse e se o botão esquerdo foi clicado neste frame
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && readyToThrow && totalThrows > 0)
        {
            Throw(); 
        }
    }

    private void Throw()
    {
        readyToThrow = false;

        // instancia o objeto para jogar
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        // Pega o componente do rigidbody 
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // Adiciona Força
        Vector3 forceToAdd = cam.transform.forward * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;

        // implement throwCooldown
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow()
    {
        readyToThrow = true; // Corrigido: 'true' minúsculo
    }
}