using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Panneau d'aide "Controles". Affiche la liste des touches clavier sur PC, ou les commandes tactiles
/// sur telephone. Met le jeu en pause pendant qu'il est ouvert (si le jeu tournait).
/// </summary>
public class ControlsHelpPanel : MonoBehaviour
{
    [SerializeField] private GameObject pcContent;
    [SerializeField] private GameObject touchContent;

    /// <summary>Vrai tant que le panneau est ouvert (PCControls l'utilise pour que Echap ferme seulement l'aide).</summary>
    public static bool IsOpen { get; private set; }

    private float previousTimeScale = 1f;
    private bool pausedByPanel;

    private void OnEnable()
    {
        IsOpen = true;
        bool mobile = PlatformInfo.IsMobileDevice();
        if (pcContent != null) pcContent.SetActive(!mobile);
        if (touchContent != null) touchContent.SetActive(mobile);

        if (Time.timeScale > 0f)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            pausedByPanel = true;
        }
    }

    private void OnDisable()
    {
        IsOpen = false;
        if (pausedByPanel)
        {
            Time.timeScale = previousTimeScale;
            pausedByPanel = false;
        }
    }

    // LateUpdate : laisse PCControls traiter Echap avant, pour ne pas declencher la pause en meme temps.
    private void LateUpdate()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
