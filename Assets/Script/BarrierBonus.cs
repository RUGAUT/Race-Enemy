using UnityEngine;

public class BarrierBonus : MonoBehaviour
{
    [SerializeField] private float duration = 5f;
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                // Essaie d'activer la barrière. 
                // Ne fonctionne que si le joueur n'en a pas déjà une.
                if (vehicleHealth.ActivateBarrier(duration))
                {
                    // L'activation a réussi : on joue le VFX et on détruit le bonus
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, transform.rotation);
                    }

                    Destroy(gameObject);
                }
                // Si ActivateBarrier renvoie false, on ignore et le bonus reste sur la route !
            }
        }
    }
}