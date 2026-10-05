using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Branche mes scripts sur le vehicule du joueur a CHAQUE chargement de scene (menu puis niveau, rejouer...).
/// RuntimeInitializeOnLoadMethod(AfterSceneLoad) ne se declenche qu'une fois, au tout premier chargement :
/// quand on arrive dans LV1 depuis le menu, il n'aurait donc rien branche.
/// sceneLoaded s'execute apres les Awake et avant les Start, ce qu'il faut pour les statistiques
/// (VehicleHealth remplit sa vie dans Start).
/// </summary>
public static class SceneHooks
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        VehicleStatsApplier.Apply();
        VehicleShield.Setup();
        VehicleAbility.Setup();
    }
}
