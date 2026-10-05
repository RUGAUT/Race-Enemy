using UnityEngine;

/// <summary>
/// Affiche les controles tactiles (joystick, boutons de tir / grenade) uniquement sur telephone / tablette.
/// Sur PC ils sont rendus invisibles et non cliquables, mais restent actifs : le clavier (PCControls)
/// continue de piloter le joystick virtuel en coulisses.
/// </summary>
public class TouchControlsVisibility : MonoBehaviour
{
    [SerializeField] private RectTransform[] touchElements;

    private bool shown;
    private bool applied;

    private void Update()
    {
        // Verifie chaque frame : un toucher detecte plus tard (ex. iPad) fait apparaitre les controles.
        bool mobile = PlatformInfo.IsMobileDevice();
        if (!applied || mobile != shown)
            Apply(mobile);
    }

    private void Apply(bool show)
    {
        shown = show;
        applied = true;

        foreach (var element in touchElements)
        {
            if (element == null) continue;
            var group = element.GetComponent<CanvasGroup>();
            if (group == null) group = element.gameObject.AddComponent<CanvasGroup>();

            group.alpha = show ? 1f : 0f;
            group.blocksRaycasts = show;
            group.interactable = true; // le joystick virtuel refuse de fonctionner s'il est "non interactable"
        }
    }
}
