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
                // Si déjà invulnérable, prolonge l'invulnérabilité ET réactive la barrière avec la durée TOTALE
                if (vehicleHealth.IsInvulnerable())
                {
                    float remainingTime = vehicleHealth.GetRemainingInvincibilityTime();
                    vehicleHealth.ExtendInvulnerability(duration);
                    vehicleHealth.ActivateBarrier(remainingTime + duration); // Durée totale = restant + nouveau
                }
                else
                {
                    // Active l'invulnérabilité et la barrière avec la durée complète
                    vehicleHealth.ActivateInvulnerability(duration);
                    vehicleHealth.ActivateBarrier(duration);
                }

                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, transform.rotation);
                }

                Destroy(gameObject);
            }
        }
    }
}