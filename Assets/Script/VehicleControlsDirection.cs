using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleControlsCorrected : MonoBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 100f; // Vitesse de rotation pour tourner le véhicule
    [SerializeField] private float tiltAngle = 10f; // Angle de penchement sur l'axe Z

    // Variables pour suivre la rotation actuelle
    private float currentZAngle = 0f; // Rotation autour de l'axe Z (penchement)

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = moveActionToUse.action.ReadValue<Vector2>();

        // Déplacement du véhicule vers l'avant ou l'arrière
        if (moveDirection.y != 0)
        {
            transform.Translate(Vector3.forward * moveDirection.y * speed * Time.deltaTime);
        }

        // Gestion de la rotation pour tourner le véhicule à gauche ou à droite
        if (moveDirection.x != 0)
        {
            transform.Rotate(Vector3.up, moveDirection.x * turnSpeed * Time.deltaTime); // Rotation autour de l'axe Y (haut)
        }

        // Gestion de la rotation pour pencher le véhicule (axe Z)
        if (moveDirection.x != 0) // Si le joystick est poussé vers la gauche ou la droite
        {
            // Incliner le véhicule selon la direction (x > 0 pour droite, x < 0 pour gauche)
            if (moveDirection.x > 0) // Pencher vers la droite
            {
                currentZAngle = Mathf.Lerp(currentZAngle, -tiltAngle, turnSpeed * Time.deltaTime);
            }
            else if (moveDirection.x < 0) // Pencher vers la gauche
            {
                currentZAngle = Mathf.Lerp(currentZAngle, tiltAngle, turnSpeed * Time.deltaTime);
            }
        }
        else // Si aucune direction n'est donnée, réinitialiser le penchement
        {
            currentZAngle = Mathf.Lerp(currentZAngle, 0, turnSpeed * Time.deltaTime);
        }

        // Appliquer la rotation combinée (Y pour direction, Z pour penchement)
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, currentZAngle);
        transform.rotation = targetRotation;
    }
}
