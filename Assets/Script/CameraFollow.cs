using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Suivi de Base")]
    [SerializeField] private Transform target; // L'objet à suivre (le véhicule)
    [Tooltip("Décalage de base quand le véhicule est au centre")]
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private float smoothSpeed = 0.125f; // Vitesse de suivi

    [Header("Limites Y")]
    [SerializeField] private float minY = 5f;
    [SerializeField] private float maxY = 20f;

    [Header("Zoom Dynamique (Bords de route)")]
    [Tooltip("La position X correspondant au centre parfait de ta route")]
    [SerializeField] private float roadCenterX = 0f;
    [Tooltip("À partir de quelle distance (sur l'axe X) la caméra doit être reculée au maximum ?")]
    [SerializeField] private float maxRoadDistance = 8f;
    [Tooltip("Le décalage AJOUTÉ quand le véhicule est sur le bord (ex: reculer de -5 en Z, monter de +2 en Y)")]
    [SerializeField] private Vector3 extraZoomOffset = new Vector3(0, 2f, -5f);

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Calcule la distance absolue entre le véhicule et le centre de la route
            float distanceFromCenter = Mathf.Abs(target.position.x - roadCenterX);

            // 2. Transforme cette distance en pourcentage (0 = au centre, 1 = au bord extrême)
            // Mathf.Clamp01 s'assure que le pourcentage ne dépasse jamais 1 (100%)
            float zoomFactor = Mathf.Clamp01(distanceFromCenter / maxRoadDistance);

            // 3. Calcule l'offset final : Offset de base + (Offset bonus * pourcentage)
            Vector3 dynamicOffset = baseOffset + (extraZoomOffset * zoomFactor);

            // 4. Calcule la position désirée avec le nouvel offset
            Vector3 desiredPosition = target.position + dynamicOffset;

            // 5. Applique le lissage (Lerp)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // 6. Limite la hauteur de la caméra entre minY et maxY
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

            // 7. Applique la position finale
            transform.position = smoothedPosition;
        }
    }
}