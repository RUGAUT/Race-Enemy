using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // L'objet à suivre (le véhicule)
    [SerializeField] private Vector3 offset; // Décalage de la caméra
    [SerializeField] private float smoothSpeed = 0.125f; // Vitesse de suivi
    [SerializeField] private float minY = 5f; // **Hauteur minimale de la caméra (ajustable dans l'inspecteur)**
    [SerializeField] private float maxY = 20f; // Hauteur maximale de la caméra (optionnel, si tu veux aussi limiter la montée)

    void LateUpdate()
    {
        if (target != null)
        {
            // Position désirée de la caméra
            Vector3 desiredPosition = target.position + offset;

            // Position lissée
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // **Limite la position Y de la caméra entre minY et maxY**
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

            // Applique la position finale
            transform.position = smoothedPosition;
        }
    }
}