using UnityEngine;

public class WheelZRotationClockwise : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }
    public enum RotationDirection { Normal, Inverse } // Nouvelle liste pour le sens

    [Header("Paramètres des Roues")]
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Configuration de l'Axe et du Sens")]
    [Tooltip("Choisis l'axe local autour duquel la roue doit tourner")]
    [SerializeField] private RotationAxis axis = RotationAxis.Z;

    [Tooltip("Choisis le sens de rotation de la roue")]
    [SerializeField] private RotationDirection direction = RotationDirection.Normal;

    private void Update()
    {
        Vector3 rotationVector = GetRotationVector();

        // Si "Inverse" est choisi, on multiplie simplement la vitesse par -1
        float currentSpeed = (direction == RotationDirection.Inverse) ? -rotationSpeed : rotationSpeed;

        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(rotationVector, currentSpeed * Time.deltaTime, Space.Self);
        }
    }

    private Vector3 GetRotationVector()
    {
        switch (axis)
        {
            case RotationAxis.X: return Vector3.right;
            case RotationAxis.Y: return Vector3.up;
            case RotationAxis.Z: return Vector3.forward;
            default: return Vector3.forward;
        }
    }
}