using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuVehicleSelection : MonoBehaviour
{
    [Header("UI : Boutons de Sélection")]
    [Tooltip("Glisse ici tes Boutons UI (Dans le même ordre que tes véhicules)")]
    [SerializeField] private Button[] vehicleButtons;

    [Header("Couleurs des Boutons")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField] private Color blinkColor = Color.yellow; // Couleur pendant le clignotement

    [Header("Animation")]
    [SerializeField] private int blinkCount = 3; // Nombre de clignotements
    [SerializeField] private float blinkSpeed = 0.1f; // Vitesse du clignotement

    private Coroutine blinkCoroutine;

    private void Start()
    {
        // 1. Récupère le dernier véhicule sélectionné (0 par défaut)
        int savedIndex = PlayerPrefs.GetInt("SelectedVehicleIndex", 0);

        // 2. Au lancement, on colorie directement le bon bouton en vert
        ResetAllButtonsColor();
        if (vehicleButtons.Length > savedIndex && vehicleButtons[savedIndex] != null)
        {
            vehicleButtons[savedIndex].GetComponent<Image>().color = selectedColor;
        }
    }

    /// <summary>
    /// Fonction à relier à tes boutons de sélection (Bouton 1 -> index 0, Bouton 2 -> index 1...)
    /// </summary>
    public void ChooseVehicle(int vehicleIndex)
    {
        // 1. Sauvegarde le choix pour la scène de jeu
        PlayerPrefs.SetInt("SelectedVehicleIndex", vehicleIndex);
        PlayerPrefs.Save();

        // 2. Lance l'animation de clignotement du bouton cliqué
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine); // Stoppe l'animation précédente si on clique très vite sur plusieurs boutons
        }
        blinkCoroutine = StartCoroutine(BlinkRoutine(vehicleIndex));
    }

    private void ResetAllButtonsColor()
    {
        // Remet tous les boutons à la couleur normale
        foreach (Button btn in vehicleButtons)
        {
            if (btn != null)
            {
                btn.GetComponent<Image>().color = normalColor;
            }
        }
    }

    private IEnumerator BlinkRoutine(int index)
    {
        // Remet tout le monde dans la couleur de base
        ResetAllButtonsColor();

        // Sécurité si l'index n'est pas bon ou que le bouton manque
        if (index < 0 || index >= vehicleButtons.Length || vehicleButtons[index] == null)
            yield break;

        // Récupère l'image de fond du bouton cliqué
        Image buttonImage = vehicleButtons[index].GetComponent<Image>();

        // Boucle de clignotement
        for (int i = 0; i < blinkCount; i++)
        {
            buttonImage.color = blinkColor;
            yield return new WaitForSeconds(blinkSpeed);

            buttonImage.color = normalColor;
            yield return new WaitForSeconds(blinkSpeed);
        }

        // À la fin de l'animation, le bouton devient vert de façon permanente
        buttonImage.color = selectedColor;
    }
}