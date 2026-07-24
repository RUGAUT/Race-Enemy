using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10; // Dégâts infligés par cet obstacle

    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet qui entre en collision est le véhicule
        if (other.CompareTag("Player")) // Assure-toi que ton véhicule a le tag "Vehicle"
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                vehicleHealth.TakeDamage(damageAmount);
            }

            // Fais disparaître l'obstacle
            Destroy(gameObject);
        }
    }

    // Méthode pour accéder aux dégâts (optionnelle)
    public int GetDamageAmount()
    {
        return damageAmount;
    }
}