using UnityEngine;
using UnityEngine.InputSystem; 

public class ShotgunShoot : MonoBehaviour
{
    [Header("References")]
    public Transform cam; 

    [Header("Settings")]
    public int totalShots = 30; 
    public float shootCooldown = 0.8f;
    
    [Header("Shotgun Spread")]
    public int pelletsPerShot = 8; 
    public float spreadAmount = 0.05f; 
    public float range = 50f; 
    public float damagePerPellet = 10f; 

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

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 spread = cam.forward + new Vector3(
                Random.Range(-spreadAmount, spreadAmount),
                Random.Range(-spreadAmount, spreadAmount),
                Random.Range(-spreadAmount, spreadAmount)
            );

            Vector3 shootDirection = spread.normalized;

            // rayyyyyyyycast
            Debug.DrawRay(cam.position, shootDirection * range, Color.red, 2f);
            

            if (Physics.Raycast(cam.position, shootDirection, out RaycastHit hit, range))
            {
                Debug.Log("Acertou: " + hit.collider.name);
            }
        }

        Invoke(nameof(ResetShoot), shootCooldown);
    }

    private void ResetShoot()
    {
        readyToShoot = true; 
    }
}