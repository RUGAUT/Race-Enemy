using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Transform vehicleTransform;
    [SerializeField] private TextMeshProUGUI distanceScoreText;
    [SerializeField] private TextMeshProUGUI zombieScoreText;

    private float initialPositionZ;
    private float distanceScore;
    private int zombieScore;

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
        if (vehicleTransform != null)
        {
            float distanceTravelled = vehicleTransform.position.z - initialPositionZ;
            distanceScore = Mathf.Max(0, distanceTravelled);
            UpdateScoreUI();
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

    // --- NOUVEAU : Fonctions pour transmettre les scores au GameManager ---
    public int GetFinalDistance()
    {
        return Mathf.FloorToInt(distanceScore);
    }

    public int GetFinalZombieScore()
    {
        return zombieScore;
    }
}