using UnityEngine;

/// <summary>
/// Petit effet d'animation pour les menus : pulsation d'echelle et flottement vertical.
/// (Script ajoute pour le MainMenu_Pro.)
/// </summary>
public class MenuPulse : MonoBehaviour
{
    [SerializeField] private float scaleAmount = 0.04f;
    [SerializeField] private float scaleSpeed = 2.5f;
    [SerializeField] private float floatAmount = 0f;
    [SerializeField] private float floatSpeed = 1.5f;

    private Vector3 baseScale;
    private Vector3 basePos;

    private void OnEnable()
    {
        baseScale = transform.localScale;
        basePos = transform.localPosition;
    }

    private void OnDisable()
    {
        transform.localScale = baseScale;
        transform.localPosition = basePos;
    }

    private void Update()
    {
        float t = Time.unscaledTime;
        transform.localScale = baseScale * (1f + Mathf.Sin(t * scaleSpeed) * scaleAmount);
        if (floatAmount > 0f)
            transform.localPosition = basePos + Vector3.up * (Mathf.Sin(t * floatSpeed) * floatAmount);
    }
}
