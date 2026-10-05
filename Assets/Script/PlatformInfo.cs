using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecte si le joueur est sur telephone / tablette (important pour le build WebGL).
///   PlatformInfo.IsMobileDevice() : true sur navigateur mobile, ou des qu'un vrai toucher est detecte
///                                   (utile pour les iPad, dont le navigateur se presente comme un ordinateur).
/// Dans l'editeur, utiliser le menu "Race Enemy > Simuler mobile dans l'editeur" pour tester.
/// </summary>
public static class PlatformInfo
{
    /// <summary>Passe a true des qu'un ecran tactile est utilise pendant la partie.</summary>
    public static bool TouchDetected { get; private set; }

    public const string SimulateMobilePref = "RaceEnemy.SimulateMobile";

    public static bool IsMobileDevice()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorPrefs.GetBool(SimulateMobilePref, false) || TouchDetected;
#else
        return Application.isMobilePlatform || TouchDetected;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateTouchWatcher()
    {
        var go = new GameObject("PlatformInfoWatcher") { hideFlags = HideFlags.HideInHierarchy };
        Object.DontDestroyOnLoad(go);
        go.AddComponent<TouchWatcher>();
    }

    private class TouchWatcher : MonoBehaviour
    {
        private void Update()
        {
            if (TouchDetected) return;
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.press.wasPressedThisFrame)
                TouchDetected = true;
        }
    }
}
