using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    // Variables pour définir les vitesses de rotation sur chaque axe
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 100f, 0f); // Vitesse de rotation (en degrés par seconde) pour chaque axe
    [SerializeField] private bool useLocalRotation = true; // Choix entre rotation locale ou mondiale

    void Update()
    {
        RotateObject();
    }

    // Fonction pour faire tourner l'objet
    private void RotateObject()
    {
        // Rotation en fonction de l'option (locale ou globale)
        if (useLocalRotation)
        {
            // Rotation locale (autour des axes locaux de l'objet)
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            // Rotation globale (autour des axes du monde)
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
