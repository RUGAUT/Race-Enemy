using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveActionToUse;

    [Header("Movement")]
    public float forwardSpeed = 10f; // Avance constante
    public float laneChangeSpeed = 5f;
    private float[] lanePositions = new float[] { -2f, 0f, 2f };
    private int currentLane = 1;
    private Vector3 targetPosition;

    [Header("Tilt (lean effect)")]
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxTiltAngle = 10f;
    private float currentZAngle = 0f;

    void Start()
    {
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void Update()
    {
        // Mouvement constant vers l'avant
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // Lecture de l'entrée du joystick
        Vector2 moveInput = moveActionToUse.action.ReadValue<Vector2>();

        // Si un mouvement horizontal est détecté
        if (moveInput.x > 0.5f && currentLane < 2)
        {
            currentLane++;
            UpdateTargetPosition();
        }
        else if (moveInput.x < -0.5f && currentLane > 0)
        {
            currentLane--;
            UpdateTargetPosition();
        }

        // Mouvement latéral fluide vers la voie cible
        Vector3 newPosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, laneChangeSpeed * Time.deltaTime);

        // Penchement du véhicule en fonction du mouvement
        float targetZAngle = 0f;
        if (transform.position.x < targetPosition.x)
        {
            targetZAngle = -maxTiltAngle; // penche vers la droite
        }
        else if (transform.position.x > targetPosition.x)
        {
            targetZAngle = maxTiltAngle; // penche vers la gauche
        }

        currentZAngle = Mathf.Lerp(currentZAngle, targetZAngle, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, currentZAngle);
    }

    private void UpdateTargetPosition()
    {
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }
}

