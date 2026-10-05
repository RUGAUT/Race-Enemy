using TMPro;
using UnityEngine;

/// <summary>
/// A placer sur le panel de victoire / de defaite : donne les pieces de la partie (une seule fois)
/// et affiche le gain en bas du panel.
/// Gain = score de zombies + bonus de victoire + bonus par etoile.
/// </summary>
public class RunRewards : MonoBehaviour
{
    [SerializeField] private bool victory;
    [SerializeField] private int victoryBonus = 100;
    [SerializeField] private int bonusPerStar = 50;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Sprite coinIcon;

    private bool given;

    private void Start()
    {
        if (given) return;
        given = true;

        int score = ScoreManager.Instance != null ? ScoreManager.Instance.GetFinalZombieScore() : 0;
        int reward = Mathf.Max(0, score);
        if (victory) reward += victoryBonus + bonusPerStar * WinStarsDisplay.LastStars;

        Economy.AddCoins(reward);
        ShowLabel(reward);
    }

    private void ShowLabel(int reward)
    {
        var row = new GameObject("Pieces Gagnees", typeof(RectTransform));
        // Accroche au bord bas du cadre du panel : suit le cadre quelle que soit la proportion d'ecran
        var window = transform.Find("Fenetre");
        row.transform.SetParent(window != null ? window : transform, false);
        var rowRt = row.GetComponent<RectTransform>();
        rowRt.anchorMin = rowRt.anchorMax = new Vector2(0.5f, 0f);
        rowRt.pivot = new Vector2(0.5f, 1f);
        rowRt.anchoredPosition = new Vector2(0f, -4f);
        rowRt.sizeDelta = new Vector2(420f, 84f);

        var iconGo = new GameObject("Icone Piece", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        iconGo.transform.SetParent(row.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0f, 0.5f);
        iconRt.anchoredPosition = new Vector2(10f, 0f);
        iconRt.sizeDelta = new Vector2(80f, 80f);
        var img = iconGo.GetComponent<UnityEngine.UI.Image>();
        img.sprite = coinIcon;
        img.raycastTarget = false;

        var go = new GameObject("Texte", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(row.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(100f, 0f);
        rt.offsetMax = Vector2.zero;

        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = font;
        t.fontSize = 62;
        t.alignment = TextAlignmentOptions.Left;
        t.color = new Color32(0xFF, 0xC8, 0x1E, 255);
        t.raycastTarget = false;
        t.text = string.Format(Loc.T("coins_earned"), reward);
    }
}
