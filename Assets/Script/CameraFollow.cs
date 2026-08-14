using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Suivi de Base")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private float smoothSpeed = 0.125f;

    [Header("Limites Y")]
    [SerializeField] private float minY = 5f;
    [SerializeField] private float maxY = 20f;

    [Header("Zoom Dynamique (Bords de route)")]
    [SerializeField] private float roadCenterX = 0f;
    [SerializeField] private float maxRoadDistance = 8f;
    [SerializeField] private Vector3 extraZoomOffset = new Vector3(0, 2f, -5f);

    // --- VARIABLES DE TREMBLEMENT (SHAKE) ---
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    /// <summary>
    /// Déclenche un tremblement de la caméra
    /// </summary>
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            float distanceFromCenter = Mathf.Abs(target.position.x - roadCenterX);
            float zoomFactor = Mathf.Clamp01(distanceFromCenter / maxRoadDistance);
            Vector3 dynamicOffset = baseOffset + (extraZoomOffset * zoomFactor);
            Vector3 desiredPosition = target.position + dynamicOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);

            // --- APPLICATION DU TREMBLEMENT ---
            if (shakeDuration > 0)
            {
                // Ajoute un décalage aléatoire dans une sphère
                Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
                smoothedPosition += randomOffset;

                // Diminue le chronomètre
                shakeDuration -= Time.deltaTime;
            }

            transform.position = smoothedPosition;
        }
    }
}