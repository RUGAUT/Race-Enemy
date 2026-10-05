using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retour visuel quand une capacite est utilisee : gros texte qui apparait au centre (ex. "TURBO !")
/// et petit message "PRET !" quand elle est de nouveau disponible. Cree son propre Canvas au premier appel.
/// </summary>
public class AbilityFeedback : MonoBehaviour
{
    private static AbilityFeedback instance;
    private TextMeshProUGUI label;
    private CanvasGroup group;
    private Coroutine running;

    public static void Show(string text, Color color)
    {
        if (instance == null)
        {
            var go = new GameObject("AbilityFeedback");
            instance = go.AddComponent<AbilityFeedback>();
            instance.Build();
        }
        instance.Play(text, color);
    }

    private void Build()
    {
        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var textGo = new GameObject("Texte", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(CanvasGroup));
        textGo.transform.SetParent(canvasGo.transform, false);
        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 200f);
        rt.sizeDelta = new Vector2(1400f, 200f);

        label = textGo.GetComponent<TextMeshProUGUI>();
        var settings = Resources.Load<ShieldSettings>("ShieldSettings");
        if (settings != null && settings.font != null) label.font = settings.font;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 110;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        group = textGo.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private void Play(string text, Color color)
    {
        label.text = text;
        label.color = color;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        const float duration = 1.1f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;
            float pop = k < 0.2f ? Mathf.Lerp(0.5f, 1.15f, k / 0.2f) : Mathf.Lerp(1.15f, 1f, (k - 0.2f) / 0.8f);
            label.rectTransform.localScale = Vector3.one * pop;
            group.alpha = k < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (k - 0.7f) / 0.3f);
            yield return null;
        }
        group.alpha = 0f;
    }
}
