using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelZRotationClockwise : MonoBehaviour
{
    [SerializeField] private Transform[] wheels; // Tableau des roues à faire tourner
    [SerializeField] private float rotationSpeed = 360f; // Vitesse de rotation des roues (degrés par seconde)

    private void Update()
    {
        // Rotation autour de l'axe Z dans le sens horaire
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime); // Rotation autour de Z
        }
    }
}
