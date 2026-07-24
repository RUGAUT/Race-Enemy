using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicKit : MonoBehaviour
{
    [SerializeField] private int healthRestoreAmount = 20; // Montant de santé restaurée
    [SerializeField] private GameObject destructionVFX; // VFX pour la destruction de la trousse de mécano

    private void OnTriggerEnter(Collider other)
    {
        // Vérifier si l'objet qui entre en contact est le véhicule
        VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
        if (vehicleHealth != null)
        {
            // Restaurer la vie du véhicule
            vehicleHealth.RestoreHealth(healthRestoreAmount);

            // Si un VFX de destruction est assigné, le faire apparaître juste avant de détruire l'objet
            if (destructionVFX != null)
            {
                Instantiate(destructionVFX, transform.position, Quaternion.identity);
            }

            // Détruire la trousse de mécano après utilisation
            Destroy(gameObject);
        }
    }
}
