using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepulsionSurCollision : MonoBehaviour
{
    [SerializeField] private float forceRepulsion = 5f; // Force de répulsion
    [SerializeField] private float distanceRepulsion = 0.5f; // Distance de repousse

    private void OnCollisionEnter(Collision collision)
    {
        // Vérifiez si le joueur touche un mur
        if (collision.gameObject.CompareTag("Mur")) // Assurez-vous que le mur a le tag "Mur"
        {
            // Calculez la direction de la répulsion
            Vector3 direction = (transform.position - collision.transform.position).normalized;

            // Appliquez une force de répulsion
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * forceRepulsion, ForceMode.Impulse);
            }
            else
            {
                // Si le Rigidbody n'est pas attaché, déplacez le joueur manuellement
                transform.position += direction * distanceRepulsion;
            }
        }
    }
}
