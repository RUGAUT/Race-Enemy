using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationAutomatiqueDesRoues : MonoBehaviour
{
    [SerializeField] private Transform[] roues; // Tableau des roues à faire tourner
    [SerializeField] private float vitesseRotation = 360f; // Vitesse de rotation des roues (degrés par seconde)

    private void Update()
    {
        // Faire tourner chaque roue
        foreach (Transform roue in roues)
        {
            roue.Rotate(Vector3.right, vitesseRotation * Time.deltaTime); // Rotation autour de l'axe X
        }
    }
}
