using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // J'ai retiré le [SerializeField] private Transform vehicleTransform; de l'Inspecteur
    [SerializeField] private TextMeshProUGUI distanceScoreText;
    [SerializeField] private TextMeshProUGUI zombieScoreText;

    private Transform activeVehicle; // Référence dynamique au véhicule actif
    private float initialPositionZ;
    private float distanceScore;
    private int zombieScore;
    private bool hasInitialized = false; // Pour enregistrer la position de départ une seule fois

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        distanceScore = 0;
        zombieScore = 0;
        UpdateScoreUI();
    }

    private void Update()
    {
        // 1. Recherche automatique du véhicule s'il est absent ou désactivé
        if (activeVehicle == null || !activeVehicle.gameObject.activeInHierarchy)
        {
            FindActiveVehicle();
        }

        // 2. Si aucun véhicule n'est actif, on attend sagement
        if (activeVehicle == null) return;

        // 3. Enregistrement du point de départ au tout premier démarrage
        if (!hasInitialized)
        {
            initialPositionZ = activeVehicle.position.z;
            hasInitialized = true;
        }

        // 4. Calcul continu de la distance basée sur le véhicule actif
        float distanceTravelled = activeVehicle.position.z - initialPositionZ;
        distanceScore = Mathf.Max(0, distanceTravelled);
        UpdateScoreUI();
    }

    // Fonction de recherche automatique via le Tag "Player"
    private void FindActiveVehicle()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            activeVehicle = playerObj.transform;
        }
    }

    public void AddZombieScore(int points)
    {
        zombieScore += points;
        UpdateScoreUI();
    }

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

    public int GetFinalDistance()
    {
        return Mathf.FloorToInt(distanceScore);
    }

    public int GetFinalZombieScore()
    {
        return zombieScore;
    }
}