using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleHealthUI : MonoBehaviour
{
    [SerializeField] private VehicleHealth vehicleHealth; // Référence au script VehicleHealth
    [SerializeField] private Slider healthBar; // Référence à la barre de vie
    [SerializeField] private Color fullHealthColor = Color.green; // Couleur pour la vie pleine
    [SerializeField] private Color halfHealthColor = Color.yellow; // Couleur pour la vie à moitié
    [SerializeField] private Color lowHealthColor = Color.red; // Couleur pour la vie faible

    private void Start()
    {
        // Initialiser la barre de vie avec la santé maximale
        if (vehicleHealth != null && healthBar != null)
        {
            healthBar.maxValue = vehicleHealth.MaxHealth; // Assurez-vous d'avoir une propriété MaxHealth dans VehicleHealth
            healthBar.value = vehicleHealth.CurrentHealth; // Initialiser à la santé actuelle
            UpdateHealthBarColor(); // Mettre à jour la couleur de la barre de vie
        }
    }

    private void Update()
    {
        // Mettre à jour la barre de vie à chaque frame
        if (vehicleHealth != null && healthBar != null)
        {
            healthBar.value = vehicleHealth.CurrentHealth; // Mettre à jour la valeur de la barre de vie
            UpdateHealthBarColor(); // Mettre à jour la couleur de la barre de vie

            // Vérifier si la santé est à zéro pour désactiver le Slider
            if (vehicleHealth.CurrentHealth <= 0)
            {
                healthBar.gameObject.SetActive(false); // Désactiver la barre de vie
            }
        }
    }

    // Méthode pour mettre à jour la couleur de la barre de vie
    private void UpdateHealthBarColor()
    {
        if (vehicleHealth.CurrentHealth > vehicleHealth.MaxHealth / 2)
        {
            healthBar.fillRect.GetComponent<Image>().color = fullHealthColor; // Couleur pour vie pleine
        }
        else if (vehicleHealth.CurrentHealth > vehicleHealth.MaxHealth / 4)
        {
            healthBar.fillRect.GetComponent<Image>().color = halfHealthColor; // Couleur pour vie à moitié
        }
        else
        {
            healthBar.fillRect.GetComponent<Image>().color = lowHealthColor; // Couleur pour vie faible
        }
    }
}
