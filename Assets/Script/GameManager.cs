using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel; // Panel de victoire
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Game Over Score Animation")]
    [SerializeField] private TextMeshProUGUI finalDistanceText;
    [SerializeField] private TextMeshProUGUI finalZombieText;

    [Header("Win Score Animation")]
    [SerializeField] private TextMeshProUGUI winFinalDistanceText; // Texte distance sur le panel Win
    [SerializeField] private TextMeshProUGUI winFinalZombieText;   // Texte zombie sur le panel Win

    [SerializeField] private float scoreAnimationDuration = 1.5f; // Durée de la montée des chiffres

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
        if (winPanel != null) winPanel.SetActive(false); // Désactive le panel Win au démarrage
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Fige le jeu

            // Lance l'animation des scores pour le Game Over
            StartCoroutine(AnimateScoreCounters(finalDistanceText, finalZombieText));
        }
    }

    public void TriggerWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f; // Fige le jeu

            // Lance l'animation des scores pour la Victoire
            StartCoroutine(AnimateScoreCounters(winFinalDistanceText, winFinalZombieText));
        }
    }

    // --- Coroutine d'animation des scores réutilisable pour les deux panels ---
    private IEnumerator AnimateScoreCounters(TextMeshProUGUI distText, TextMeshProUGUI zombText)
    {
        int targetDistance = 0;
        int targetZombie = 0;

        if (ScoreManager.Instance != null)
        {
            targetDistance = ScoreManager.Instance.GetFinalDistance();
            targetZombie = ScoreManager.Instance.GetFinalZombieScore();
        }

        float elapsedTime = 0f;

        while (elapsedTime < scoreAnimationDuration)
        {
            // Utilise unscaledDeltaTime car timeScale est à 0 !
            elapsedTime += Time.unscaledDeltaTime;

            float progress = elapsedTime / scoreAnimationDuration;
            float easeOutProgress = 1f - Mathf.Pow(1f - progress, 3);

            int currentDistance = Mathf.RoundToInt(Mathf.Lerp(0, targetDistance, easeOutProgress));
            int currentZombie = Mathf.RoundToInt(Mathf.Lerp(0, targetZombie, easeOutProgress));

            if (distText != null) distText.text = "Distance: " + currentDistance + "m";
            if (zombText != null) zombText.text = "Zombies Tués: " + currentZombie;

            yield return null;
        }

        // Sécurité : affiche le score final exact à la fin
        if (distText != null) distText.text = "Distance: " + targetDistance + "m";
        if (zombText != null) zombText.text = "Zombies Tués: " + targetZombie;
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