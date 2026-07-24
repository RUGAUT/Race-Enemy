using UnityEngine;
using TMPro; // Assure-toi d'importer le namespace TextMeshPro

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText; // Référence au composant TextMeshProUGUI
    [SerializeField] private float updateInterval = 0.5f; // Intervalle de mise à jour en secondes

    private float timeLeft; // Temps restant avant la prochaine mise à jour
    private int frameCount; // Compteur de frames
    private float currentFPS; // FPS actuel

    private void Start()
    {
        // Si fpsText n'est pas assigné, essaie de le trouver automatiquement
        if (fpsText == null)
        {
            fpsText = GetComponent<TextMeshProUGUI>();
            if (fpsText == null)
            {
                fpsText = FindObjectOfType<TextMeshProUGUI>();
            }
        }

        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        frameCount++;

        // Quand le temps est écoulé, calcule le FPS
        if (timeLeft <= 0f)
        {
            currentFPS = frameCount / updateInterval;
            frameCount = 0;
            timeLeft = updateInterval;

            // Met à jour le texte de l'UI
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {Mathf.RoundToInt(currentFPS)}";
            }
        }
    }
}