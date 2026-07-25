using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private int joystickID = 0; // ID du Virtual Joystick à utiliser
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 5f; // Vitesse de rotation pour pencher le véhicule
    [SerializeField] private float maxTurnAngle = 15f; // Angle maximum de rotation latérale
    [SerializeField] private float tiltAngle = 10f; // Angle de penchement sur l'axe Z

    private float currentYAngle = 0f; // Rotation autour de l'axe Y
    private float currentZAngle = 0f; // Rotation autour de l'axe Z (penchement)

    void Update()
    {
        // Récupère l'instance du Virtual Joystick avec l'ID spécifié
        Terresquall.VirtualJoystick joystick = Terresquall.VirtualJoystick.GetInstance(joystickID);

        // Si le joystick n'existe pas, on ne fait rien
        if (joystick == null)
        {
            Debug.LogWarning("Aucun Virtual Joystick trouvé avec l'ID : " + joystickID);
            return;
        }

        // Récupère les entrées du joystick (méthode d'instance)
        Vector2 moveDirection = joystick.GetAxis();

        // --- Déplacement ---
        if (moveDirection.y > 0) // Vers le haut = gauche (inversé)
        {
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        else if (moveDirection.y < 0) // Vers le bas = droite (inversé)
        {
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }

        // --- Penchement (axe Z) ---
        if (moveDirection.y != 0)
        {
            currentZAngle = Mathf.Lerp(
                currentZAngle,
                moveDirection.y > 0 ? tiltAngle : -tiltAngle,
                turnSpeed * Time.deltaTime
            );
        }
        else
        {
            currentZAngle = Mathf.Lerp(currentZAngle, 0, turnSpeed * Time.deltaTime);
        }

        // --- Rotation (axe Y) ---
        if (moveDirection.x != 0)
        {
            currentYAngle = Mathf.Clamp(
                currentYAngle + (moveDirection.x > 0 ? 1 : -1) * turnSpeed * Time.deltaTime,
                -maxTurnAngle,
                maxTurnAngle
            );
        }
        else
        {
            currentYAngle = Mathf.MoveTowards(currentYAngle, 0, turnSpeed * Time.deltaTime);
        }

        // Applique la rotation combinée
        transform.rotation = Quaternion.Euler(0, currentYAngle, currentZAngle);
    }
}