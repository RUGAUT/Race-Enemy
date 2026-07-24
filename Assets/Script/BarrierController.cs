using System.Collections;
using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [SerializeField] private float duration = 5f; // Durée de la barrière
    private Transform vehicleTransform; // Référence au transform du véhicule

    public void Initialize(Transform vehicle)
    {
        vehicleTransform = vehicle; // Référencer le transform du véhicule
        transform.position = vehicleTransform.position + vehicleTransform.forward; // Positionner la barrière juste devant le véhicule

        // Démarrer la coroutine pour désactiver la barrière après la durée
        StartCoroutine(DisableBarrier());
    }

    private IEnumerator DisableBarrier()
    {
        // Attendre la durée spécifiée
        yield return new WaitForSeconds(duration);

        // Détruire le GameObject de la barrière
        Destroy(gameObject);
        Debug.Log("Barrière désactivée !");
    }

    private void Update()
    {
        if (vehicleTransform != null)
        {
            // Met à jour la position de la barrière pour suivre le véhicule
            transform.position = vehicleTransform.position + vehicleTransform.forward; // Ajuster la position de la barrière
        }
    }
}
