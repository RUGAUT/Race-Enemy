using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Transform vehicleTransform; // Référence au véhicule
    [SerializeField] private Text distanceScoreText; // Référence à l'UI pour afficher le score de distance
    [SerializeField] private Text zombieScoreText; // Référence à l'UI pour afficher le score des zombies
    private float initialPositionZ; // Position initiale du véhicule sur l'axe Z
    private float distanceScore; // Score basé sur la distance parcourue
    private int zombieScore; // Score basé sur le nombre de zombies écrasés

    private void Start()
    {
        // Stocker la position de départ du véhicule
        if (vehicleTransform != null)
        {
            initialPositionZ = vehicleTransform.position.z;
        }
        distanceScore = 0;
        zombieScore = 0;
        UpdateScoreUI();
    }

    private void Update()
    {
        // Calculer le score en fonction de la distance parcourue sur l'axe Z
        if (vehicleTransform != null)
        {
            float distanceTravelled = vehicleTransform.position.z - initialPositionZ;
            distanceScore = Mathf.Max(0, distanceTravelled); // S'assurer que le score de distance reste positif
            UpdateScoreUI(); // Mettre à jour l'affichage des scores
        }
    }

    // Méthode pour ajouter des points au score des zombies
    public void AddZombieScore(int points)
    {
        zombieScore += points; // Ajouter des points au score des zombies
        UpdateScoreUI(); // Mettre à jour l'affichage des scores
    }

    // Mettre à jour le texte de l'UI avec les scores actuels
    private void UpdateScoreUI()
    {
        if (distanceScoreText != null)
        {
            distanceScoreText.text = "Distance Score: " + Mathf.FloorToInt(distanceScore).ToString();
        }

        if (zombieScoreText != null)
        {
            zombieScoreText.text = "Zombie Score: " + zombieScore.ToString();
        }
    }
}
