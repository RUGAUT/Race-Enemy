using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CorrectVehicleOrientation : MonoBehaviour
{
    [SerializeField] private InputActionReference moveActionToUse;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 180f; // Vitesse de rotation
    private Vector3 moveDirection; // Stocke la direction de déplacement

    // Update is called once per frame
    void Update()
    {
        Vector2 inputDirection = moveActionToUse.action.ReadValue<Vector2>();

        // Gérer la direction du véhicule
        if (inputDirection != Vector2.zero)
        {
            // Calculer la direction de mouvement dans l'espace du monde
            moveDirection = new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;

            // Créer la rotation pour que le véhicule fasse face à la direction de déplacement
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            // Appliquer la rotation en lissant avec une vitesse de rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Déplacement du véhicule dans la direction actuelle
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}
