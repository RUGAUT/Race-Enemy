using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // NOUVEAU : Requis pour les TextMeshPro dans l'interface Game Over
using System.Collections; // NOUVEAU : Requis pour les Coroutines

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Game Over Score Animation")]
    [SerializeField] private TextMeshProUGUI finalDistanceText; // Texte pour la distance finale
    [SerializeField] private TextMeshProUGUI finalZombieText;   // Texte pour le score zombie final
    [SerializeField] private float scoreAnimationDuration = 1.5f; // Durée de la montée des chiffres (en secondes)

    private bool isPaused = false;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Fige le jeu

            // NOUVEAU : Lance l'animation des scores
            StartCoroutine(AnimateScoreCounters());
        }
    }

    // --- Coroutine d'animation des scores (façon Arcade) ---
    private IEnumerator AnimateScoreCounters()
    {
        // 1. Récupère les scores finaux depuis le ScoreManager
        int targetDistance = 0;
        int targetZombie = 0;

        if (ScoreManager.Instance != null)
        {
            targetDistance = ScoreManager.Instance.GetFinalDistance();
            targetZombie = ScoreManager.Instance.GetFinalZombieScore();
        }

        float elapsedTime = 0f;

        // 2. Fait monter les chiffres progressivement
        while (elapsedTime < scoreAnimationDuration)
        {
            // On utilise unscaledDeltaTime car timeScale est à 0 !
            elapsedTime += Time.unscaledDeltaTime;

            // Calcul de la progression (de 0 à 1)
            float progress = elapsedTime / scoreAnimationDuration;

            // Effet "Ease-Out" (ralentit à la fin) pour un rendu plus stylé
            float easeOutProgress = 1f - Mathf.Pow(1f - progress, 3);

            // Interpolation des valeurs
            int currentDistance = Mathf.RoundToInt(Mathf.Lerp(0, targetDistance, easeOutProgress));
            int currentZombie = Mathf.RoundToInt(Mathf.Lerp(0, targetZombie, easeOutProgress));

            // Mise à jour de l'UI
            if (finalDistanceText != null) finalDistanceText.text = "Distance: " + currentDistance + "m";
            if (finalZombieText != null) finalZombieText.text = "Zombies Tués: " + currentZombie;

            // Attend la frame suivante en temps réel
            yield return null;
        }

        // 3. Sécurité : S'assure qu'à la fin de l'animation, on affiche exactement le bon score final
        if (finalDistanceText != null) finalDistanceText.text = "Distance: " + targetDistance + "m";
        if (finalZombieText != null) finalZombieText.text = "Zombies Tués: " + targetZombie;
    }

    public void OnRestartButtonClick()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnLoadSceneButtonClick(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OnPauseButtonClick()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
    }

    public void OnResumeButtonClick()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }
}