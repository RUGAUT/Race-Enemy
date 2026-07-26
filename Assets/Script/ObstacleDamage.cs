using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10; // Dégâts infligés par cet obstacle

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                if (vehicleHealth.HasActiveBarrier)
                {
                    // 1. Le joueur a la barrière : on lui demande de jouer le VFX
                    vehicleHealth.PlayBarrierDestroyVFX(transform.position);

                    // 2. L'obstacle se détruit LUI-MÊME
                    Destroy(gameObject);
                }
                else
                {
                    // 3. Pas de barrière : le joueur prend des dégâts
                    vehicleHealth.TakeDamage(damageAmount);

                    // 4. L'obstacle se détruit LUI-MÊME
                    Destroy(gameObject);
                }
            }
        }
    }

    public int GetDamageAmount()
    {
        return damageAmount;
    }
}