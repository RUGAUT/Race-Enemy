using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retours visuels du gameplay (ne modifie aucun script existant, lit seulement leur etat) :
///  - flash rouge + petit tremblement de camera quand le vehicule perd de la vie
///  - bandeau "BOSS EN APPROCHE !" quand un boss apparait
///  - compteur de COMBO quand on enchaine les zombies (reset apres quelques secondes sans kill)
/// L'interface est creee par le script lui-meme dans son propre Canvas.
/// </summary>
public class GameFeedback : MonoBehaviour
{
    [Header("Apparence (assignees par l'outil Race Enemy > Build Game Feedback)")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private Sprite bannerSprite;

    [Header("Degats")]
    [SerializeField] private float flashMaxAlpha = 0.25f;
    [SerializeField] private float flashFadeSpeed = 2.2f;
    [Tooltip("Les gros degats (kamikaze...) font deja trembler la camera : on ne l'ecrase pas.")]
    [SerializeField] private int strongHitThreshold = 30;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.12f;

    [Header("Combo")]
    [SerializeField] private float comboWindow = 2.5f;
    [SerializeField] private int comboMinToShow = 3;

    [Header("Boss")]
    [SerializeField] private float bannerDuration = 2.4f;

    /// <summary>Zombies elimines pendant la partie (boss exclu). Utilise pour les etoiles de victoire.</summary>
    public static int ZombieKills { get; private set; }

    [Tooltip("Un gain de score a partir de cette valeur est considere comme un boss (non compte dans ZombieKills).")]
    [SerializeField] private int bossPointsThreshold = 100;

    private Image flash;
    private RectTransform banner;
    private CanvasGroup bannerGroup;
    private TextMeshProUGUI bannerText;
    private TextMeshProUGUI comboText;

    private VehicleHealth vehicle;
    private int lastHealth;
    private CameraFollow cam;

    private bool bossWasPresent;
    private float bannerTimer = -1f;

    private int lastScore;
    private int combo;
    private float comboTimer;
    private float comboPunch;

    private float nextSearchTime;

    private void Start()
    {
        ZombieKills = 0;
        BuildUI();
    }

    private void Update()
    {
        bool running = Time.timeScale > 0f;

        if (Time.unscaledTime >= nextSearchTime)
        {
            nextSearchTime = Time.unscaledTime + 0.25f;
            FindTargets();
        }

        if (running)
        {
            CheckDamage();
            CheckBoss();
            CheckCombo();
        }

        UpdateFlash(running);
        UpdateBanner(running);
        UpdateCombo(running);
    }

    // ---------------------------------------------------------------- detection

    private void FindTargets()
    {
        if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
        {
            vehicle = FindFirstObjectByType<VehicleHealth>();
            lastHealth = vehicle != null ? vehicle.CurrentHealth : 0;
        }
        if (cam == null) cam = FindFirstObjectByType<CameraFollow>();
    }

    private void CheckDamage()
    {
        if (vehicle == null) return;

        int health = vehicle.CurrentHealth;
        int lost = lastHealth - health;
        lastHealth = health;
        if (lost <= 0) return;

        flash.color = new Color(0.9f, 0.05f, 0.05f, flashMaxAlpha);
        if (cam != null && lost < strongHitThreshold)
            cam.TriggerShake(shakeDuration, shakeMagnitude);
    }

    private void CheckBoss()
    {
        bool present = FindFirstObjectByType<BossHealth>() != null;
        if (present && !bossWasPresent)
            bannerTimer = 0f;
        bossWasPresent = present;
    }

    private void CheckCombo()
    {
        var score = ScoreManager.Instance;
        if (score == null) return;

        int current = score.GetFinalZombieScore();
        if (current > lastScore)
        {
            int gained = current - lastScore;
            if (gained < bossPointsThreshold) ZombieKills += gained;
            combo++;
            comboTimer = comboWindow;
            comboPunch = 1f;
        }
        lastScore = current;

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f) combo = 0;
        }
    }

    // ----------------------------------------------------------------- affichage

    private void UpdateFlash(bool running)
    {
        Color c = flash.color;
        c.a = Mathf.MoveTowards(c.a, 0f, flashFadeSpeed * flashMaxAlpha * Time.unscaledDeltaTime);
        if (!running) c.a = 0f;
        flash.color = c;
    }

    private void UpdateBanner(bool running)
    {
        if (bannerTimer < 0f || !running)
        {
            bannerGroup.alpha = 0f;
            return;
        }

        bannerTimer += Time.deltaTime;
        float t = bannerTimer / bannerDuration;
        if (t >= 1f)
        {
            bannerTimer = -1f;
            bannerGroup.alpha = 0f;
            return;
        }

        bannerText.text = Loc.T("boss_incoming");

        // apparition avec rebond sur le premier quart, puis fondu sur la fin
        float pop = Mathf.Clamp01(t / 0.2f);
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float eased = 1f + c3 * Mathf.Pow(pop - 1f, 3) + c1 * Mathf.Pow(pop - 1f, 2);
        banner.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, eased);
        bannerGroup.alpha = t > 0.8f ? (1f - t) / 0.2f : 1f;
    }

    private void UpdateCombo(bool running)
    {
        bool show = running && combo >= comboMinToShow && comboTimer > 0f;
        if (!show)
        {
            comboText.alpha = 0f;
            return;
        }

        comboPunch = Mathf.MoveTowards(comboPunch, 0f, Time.deltaTime * 4f);
        comboText.text = Loc.T("combo") + " x" + combo;
        comboText.rectTransform.localScale = Vector3.one * (1f + 0.35f * comboPunch);
        // le texte s'efface pendant la derniere seconde de la fenetre
        comboText.alpha = Mathf.Clamp01(comboTimer);
    }

    // ----------------------------------------------------------------------- UI

    private void BuildUI()
    {
        var canvasGo = new GameObject("GameFeedbackCanvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Flash plein ecran
        var flashGo = NewRect("Flash", canvasGo.transform);
        Stretch(flashGo);
        flash = flashGo.gameObject.AddComponent<Image>();
        flash.color = new Color(0.9f, 0.05f, 0.05f, 0f);
        flash.raycastTarget = false;

        // Bandeau boss
        banner = NewRect("Bandeau Boss", canvasGo.transform);
        banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 0.78f);
        banner.sizeDelta = new Vector2(1000, 190);
        var bannerImage = banner.gameObject.AddComponent<Image>();
        bannerImage.sprite = bannerSprite;
        bannerImage.type = bannerSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        bannerImage.color = bannerSprite != null ? Color.white : new Color(0.8f, 0.1f, 0.1f, 0.9f);
        bannerImage.raycastTarget = false;
        bannerGroup = banner.gameObject.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;
        var bannerLabel = NewRect("Texte", banner);
        Stretch(bannerLabel);
        bannerLabel.offsetMin = new Vector2(20, 14);
        bannerText = bannerLabel.gameObject.AddComponent<TextMeshProUGUI>();
        StyleText(bannerText, 92, Color.white);

        // Compteur de combo, sous le compteur de zombies
        var comboRect = NewRect("Combo", canvasGo.transform);
        comboRect.anchorMin = comboRect.anchorMax = new Vector2(0.5f, 1f);
        comboRect.anchoredPosition = new Vector2(-150f, -150f);
        comboRect.sizeDelta = new Vector2(600, 90);
        comboText = comboRect.gameObject.AddComponent<TextMeshProUGUI>();
        StyleText(comboText, 64, new Color32(0xFF, 0xC8, 0x1E, 255));
        comboText.alpha = 0f;
    }

    private void StyleText(TextMeshProUGUI t, float size, Color color)
    {
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
    }

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
