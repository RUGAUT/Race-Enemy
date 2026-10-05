using UnityEngine;

/// <summary>
/// Animation d'apparition (zoom avec rebond) qui fonctionne meme quand Time.timeScale = 0.
/// Utilise pour les panels de fin de partie (Win / Echec).
/// </summary>
public class PanelPopIn : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float startScale = 0.5f;

    private Vector3 targetScale;
    private float startTime;
    private bool initialized;

    private void OnEnable()
    {
        if (!initialized)
        {
            targetScale = transform.localScale;
            initialized = true;
        }
        startTime = Time.unscaledTime;
        transform.localScale = targetScale * startScale;
    }

    private void OnDisable()
    {
        if (initialized) transform.localScale = targetScale;
    }

    private void Update()
    {
        float t = (Time.unscaledTime - startTime - delay) / duration;
        if (t <= 0f) { transform.localScale = targetScale * startScale; return; }
        if (t >= 1f) { transform.localScale = targetScale; return; }

        // easeOutBack
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float e = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
        transform.localScale = targetScale * Mathf.LerpUnclamped(startScale, 1f, e);
    }
}
