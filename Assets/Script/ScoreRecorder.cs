using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enregistre le record de zombies elimines pour l'afficher dans le menu (MenuStatsDisplay).
/// Se cree tout seul au lancement du jeu et lit ScoreManager sans le modifier.
/// </summary>
public class ScoreRecorder : MonoBehaviour
{
    private int lastZombieScore;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("ScoreRecorder");
        DontDestroyOnLoad(go);
        go.AddComponent<ScoreRecorder>();
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Update()
    {
        if (ScoreManager.Instance == null) return;

        lastZombieScore = ScoreManager.Instance.GetFinalZombieScore();
        SaveIfRecord();
    }

    private void SaveIfRecord()
    {
        if (lastZombieScore > PlayerPrefs.GetInt(MenuStatsDisplay.BestZombieScoreKey, 0))
            PlayerPrefs.SetInt(MenuStatsDisplay.BestZombieScoreKey, lastZombieScore);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SaveIfRecord();
        PlayerPrefs.Save();
        lastZombieScore = 0;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveIfRecord();
        PlayerPrefs.Save();
    }
}
