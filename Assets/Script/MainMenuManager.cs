using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuration des Panels")]
    [SerializeField] private GameObject currentActivePanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("UI du Chargement (Optionnel)")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Temps de Chargement Artificiel")]
    [Tooltip("Temps minimum de chargement en secondes")]
    [SerializeField] private float minLoadingTime = 2f;

    [Tooltip("Temps maximum de chargement en secondes")]
    [SerializeField] private float maxLoadingTime = 5f;

    private void Start()
    {
        if (currentActivePanel != null) currentActivePanel.SetActive(true);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void SwitchPanel(GameObject panelToOpen)
    {
        if (panelToOpen == null) return;

        if (currentActivePanel != null) currentActivePanel.SetActive(false);
        panelToOpen.SetActive(true);
        currentActivePanel = panelToOpen;
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (currentActivePanel != null) currentActivePanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);

        StartCoroutine(LoadSceneAsyncRoutine(sceneName));
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        yield return null;

        // 1. Lance le chargement en arrière-plan
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // ASTUCE : Empêche Unity de basculer automatiquement sur la scène quand elle est prête
        asyncLoad.allowSceneActivation = false;

        // 2. Tire une durée aléatoire entre minLoadingTime et maxLoadingTime (ex: 3.4 secondes)
        float targetDuration = Random.Range(minLoadingTime, maxLoadingTime);
        float elapsedTime = 0f;

        // 3. Boucle tant que la durée visuelle n'est pas écoulée OU que le vrai chargement n'est pas prêt (0.9)
        while (elapsedTime < targetDuration || asyncLoad.progress < 0.9f)
        {
            elapsedTime += Time.deltaTime;

            // Progression visuelle basée sur le temps (0.0 à 1.0)
            float timeProgress = Mathf.Clamp01(elapsedTime / targetDuration);

            // Progression réelle de Unity (0.0 à 1.0)
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Sécurité : On prend le plus petit des deux au cas où une machine lente prend plus de temps que prévu
            float displayProgress = Mathf.Min(timeProgress, realProgress);

            // Mise à jour UI
            if (progressBar != null) progressBar.value = displayProgress;
            if (progressText != null) progressText.text = $"Chargement... {(displayProgress * 100f):F0}%";

            yield return null;
        }

        // 4. Force la barre à 100%
        if (progressBar != null) progressBar.value = 1f;
        if (progressText != null) progressText.text = "Chargement... 100%";

        // Petit délai imperceptible de 0.2s à 100% pour que le visuel soit propre
        yield return new WaitForSeconds(0.2f);

        // 5. Débloque le passage à la scène suivante !
        asyncLoad.allowSceneActivation = true;
    }
}