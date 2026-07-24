using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float turnSpeed = 5f; // Vitesse de rotation pour pencher le véhicule
    [SerializeField] private float maxTurnAngle = 15f; // Angle maximum de rotation latérale (penchement)
    [SerializeField] private float tiltAngle = 10f; // Angle de penchement sur l'axe Z

    // Variable pour suivre la rotation actuelle sur l'axe Y (direction) et Z (penchement)
    private float currentYAngle = 0f; // Rotation autour de l'axe Y
    private float currentZAngle = 0f; // Rotation autour de l'axe Z (penchement)

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = moveActionToUse.action.ReadValue<Vector2>();

        // Déplacement du véhicule (inversé : haut = gauche, bas = droite)
        if (moveDirection.y > 0) // Vers le haut = se déplacer à gauche (inversé)
        {
            transform.Translate(-speed * Time.deltaTime, 0, 0); // Déplacement à gauche
        }
        else if (moveDirection.y < 0) // Vers le bas = se déplacer à droite (inversé)
        {
            transform.Translate(speed * Time.deltaTime, 0, 0); // Déplacement à droite
        }

        // Gestion de la rotation pour pencher le véhicule (axe Z)
        if (moveDirection.y != 0) // Si le joystick est poussé vers le haut ou le bas
        {
            // Incliner le véhicule selon la direction (y > 0 pour gauche, y < 0 pour droite)
            if (moveDirection.y > 0) // Pencher vers la gauche
            {
                currentZAngle = Mathf.Lerp(currentZAngle, tiltAngle, turnSpeed * Time.deltaTime);
            }
            else if (moveDirection.y < 0) // Pencher vers la droite
            {
                currentZAngle = Mathf.Lerp(currentZAngle, -tiltAngle, turnSpeed * Time.deltaTime);
            }
        }
        else // Si aucune direction n'est donnée, réinitialiser le penchement
        {
            currentZAngle = Mathf.Lerp(currentZAngle, 0, turnSpeed * Time.deltaTime);
        }

        // Gestion de la rotation autour de l'axe Y
        if (moveDirection.x != 0) // S'il y a un mouvement latéral
        {
            if (moveDirection.x > 0) // Rotation à droite
            {
                currentYAngle = Mathf.Clamp(currentYAngle + turnSpeed * Time.deltaTime, 0, maxTurnAngle);
            }
            else if (moveDirection.x < 0) // Rotation à gauche
            {
                currentYAngle = Mathf.Clamp(currentYAngle - turnSpeed * Time.deltaTime, -maxTurnAngle, 0);
            }
        }
        else // Réinitialiser la rotation Y lorsque le véhicule ne tourne pas
        {
            currentYAngle = Mathf.MoveTowards(currentYAngle, 0, turnSpeed * Time.deltaTime);
        }

        // Appliquer la rotation combinée (Y pour direction, Z pour penchement)
        Quaternion targetRotation = Quaternion.Euler(0, currentYAngle, currentZAngle);
        transform.rotation = targetRotation;
    }
}
