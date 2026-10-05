using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cree le nouveau HUD (barre de vie, compteur de zombies, jauge de zone) dans la scene ouverte,
/// reconnecte VehicleHealthUI / ScoreManager / ZoneSpawnerManager, et desactive les anciens elements
/// (sans les supprimer).
/// </summary>
public static class HudBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/";
    private static TMP_FontAsset font;
    private static Sprite sFrame, sBarBg, sBarFill, sZoneBg, sZoneFill, sHeart, sSkull, sBoss, sMarker;

    [MenuItem("Race Enemy/Build HUD")]
    public static void Build()
    {
        Load();
        var canvasGo = GameObject.Find("Canvas");
        var health = Object.FindFirstObjectByType<VehicleHealthUI>();
        var score = Object.FindFirstObjectByType<ScoreManager>();
        var zone = Object.FindFirstObjectByType<ZoneSpawnerManager>();
        if (canvasGo == null || health == null || score == null || zone == null)
        {
            Debug.LogError("[HUD] Canvas, VehicleHealthUI, ScoreManager ou ZoneSpawnerManager introuvable.");
            return;
        }
        RectTransform root = canvasGo.GetComponent<RectTransform>();

        // Anciens elements, retrouves via les references des scripts.
        var oldHealth = new SerializedObject(health).FindProperty("healthBar").objectReferenceValue as Slider;
        var oldScoreText = new SerializedObject(score).FindProperty("zombieScoreText").objectReferenceValue as TextMeshProUGUI;
        var oldZone = new SerializedObject(zone).FindProperty("zoneDurationSlider").objectReferenceValue as Slider;

        RemoveOldPro(root, "HUD Vie Pro");
        RemoveOldPro(root, "HUD Zombies Pro");
        RemoveOldPro(root, "HUD Zone Pro");

        int index = oldScoreText != null ? oldScoreText.transform.parent.GetSiblingIndex() : 0;

        // ---------- Barre de vie (haut gauche) ----------
        var vieRoot = Img("HUD Vie Pro", root, null, new Color(0, 0, 0, 0));
        vieRoot.GetComponent<Image>().raycastTarget = false;
        Place(vieRoot, 0, 1, 0, 1, new Vector2(40, -34), new Vector2(560, 90));
        Slider hp = NewSlider("Barre de Vie", vieRoot, sBarBg, sBarFill, Color.white, Color.green, new Vector2(30, 0), new Vector2(430, 50), false);
        var heart = Img("Coeur", (RectTransform)hp.transform, sHeart, new Color32(0xFF, 0x4D, 0x5E, 255));
        Place(heart, 0, 0.5f, 0.5f, 0.5f, new Vector2(0, 2), new Vector2(86, 86));
        heart.GetComponent<Image>().preserveAspect = true;
        heart.GetComponent<Image>().raycastTarget = false;
        heart.SetAsLastSibling();

        // ---------- Compteur zombies (haut centre) ----------
        var zRoot = Img("HUD Zombies Pro", root, sFrame, new Color(0.03f, 0.09f, 0.23f, 0.85f));
        zRoot.GetComponent<Image>().type = Image.Type.Sliced;
        zRoot.GetComponent<Image>().raycastTarget = false;
        Place(zRoot, 0.5f, 1, 0.5f, 1, new Vector2(-150, -34), new Vector2(430, 80));
        var skull = Img("Icone Crane", zRoot, sSkull, Color.white);
        Place(skull, 0, 0.5f, 0.5f, 0.5f, new Vector2(48, 0), new Vector2(64, 64));
        skull.GetComponent<Image>().preserveAspect = true;
        skull.GetComponent<Image>().raycastTarget = false;
        var zTxt = Txt("Texte Zombies", zRoot, "Zombie Score: 0", 40, Color.white);
        Place(zTxt, 0, 0.5f, 0, 0.5f, new Vector2(92, 3), new Vector2(325, 70));
        var tmp = zTxt.GetComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 24;
        tmp.fontSizeMax = 40;

        // ---------- Jauge de zone (haut centre-droit) ----------
        var zoneRoot = Img("HUD Zone Pro", root, null, new Color(0, 0, 0, 0));
        zoneRoot.GetComponent<Image>().raycastTarget = false;
        Place(zoneRoot, 0.5f, 1, 0.5f, 1, new Vector2(420, -34), new Vector2(620, 90));
        Slider zs = NewSlider("Jauge Zone", zoneRoot, sZoneBg, sZoneFill, Color.white, Color.white, new Vector2(-30, 0), new Vector2(500, 40), true);
        var boss = Img("Icone Boss", zoneRoot, sBoss, new Color32(0xFF, 0x3B, 0x3B, 255));
        Place(boss, 1, 0.5f, 0.5f, 0.5f, new Vector2(-30, 2), new Vector2(86, 86));
        boss.GetComponent<Image>().preserveAspect = true;
        boss.GetComponent<Image>().raycastTarget = false;

        // Ordre : meme niveau que l'ancien compteur (donc sous les panels de fin / pause).
        vieRoot.SetSiblingIndex(index);
        zRoot.SetSiblingIndex(index);
        zoneRoot.SetSiblingIndex(index);

        // ---------- Reconnexion ----------
        SetRef(health, "healthBar", hp);
        SetRef(score, "zombieScoreText", tmp);
        SetRef(zone, "zoneDurationSlider", zs);

        // Anciens elements : desactives (pas supprimes).
        if (oldHealth != null) oldHealth.gameObject.SetActive(false);
        if (oldScoreText != null) oldScoreText.transform.parent.gameObject.SetActive(false);
        if (oldZone != null) oldZone.gameObject.SetActive(false);
        zs.gameObject.SetActive(false); // le ZoneSpawnerManager l'active au bon moment

        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        EditorSceneManager.SaveScene(canvasGo.scene);
        Debug.Log("[HUD] Nouveau HUD cree et scripts reconnectes.");
    }

    private static Slider NewSlider(string name, RectTransform parent, Sprite bg, Sprite fill, Color bgColor, Color fillColor, Vector2 pos, Vector2 size, bool withMarker)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        Place(rt, 0, 0.5f, 0, 0.5f, pos, size);
        if (!withMarker) rt.anchoredPosition = pos + new Vector2(0, 0);

        var back = Img("Fond", rt, bg, bgColor);
        back.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(back);

        var fa = new GameObject("Zone Remplissage", typeof(RectTransform)).GetComponent<RectTransform>();
        fa.SetParent(rt, false);
        Stretch(fa);
        fa.offsetMin = new Vector2(6, 6);
        fa.offsetMax = new Vector2(-6, -6);
        var fl = Img("Remplissage", fa, fill, fillColor);
        fl.GetComponent<Image>().type = Image.Type.Sliced;
        fl.anchorMin = Vector2.zero;
        fl.anchorMax = new Vector2(0, 1);
        fl.offsetMin = Vector2.zero;
        fl.offsetMax = Vector2.zero;

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fl;
        slider.targetGraphic = back.GetComponent<Image>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.direction = Slider.Direction.LeftToRight;

        if (withMarker)
        {
            var area = new GameObject("Zone Poignee", typeof(RectTransform)).GetComponent<RectTransform>();
            area.SetParent(rt, false);
            Stretch(area);
            area.offsetMin = new Vector2(10, 0);
            area.offsetMax = new Vector2(-10, 0);
            var handle = Img("Poignee", area, sMarker, Color.white);
            handle.sizeDelta = new Vector2(60, 60);
            handle.GetComponent<Image>().preserveAspect = true;
            handle.GetComponent<Image>().raycastTarget = false;
            slider.handleRect = handle;
        }
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
        return slider;
    }

    private static void Load()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        sFrame = S("Frame/BasicFrame_Round20.png");
        sBarBg = S("Slider/Slider_Basic04_Bg.png");
        sBarFill = S("Slider/Slider_Basic04_Fill_White.png");
        sZoneBg = S("Slider/Slider_Basic03_Bg.png");
        sZoneFill = S("Slider/Slider_Basic03_Fill_Yellow.png");
        sHeart = S("Icon_PictoIcons/128/Pictoicon_Life.Png");
        sSkull = S("IconMisc/Icon_StatsIcon_Skeleton.Png");
        sBoss = S("IconMisc/Icon_PictoIcon_Boss_Stage_f.png");
        sMarker = S("Slider/Slider_Icon05_Icon.png");
    }

    private static Sprite S(string rel)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(LL + rel);
        if (s == null) Debug.LogWarning("[HUD] Sprite introuvable : " + rel);
        return s;
    }

    private static void RemoveOldPro(RectTransform root, string name)
    {
        var t = root.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject); // uniquement mes propres elements d'une execution precedente
    }

    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform Img(string name, RectTransform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        return go.GetComponent<RectTransform>();
    }

    private static RectTransform Txt(string name, RectTransform parent, string text, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, float ax, float ay, float px, float py, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
        rt.pivot = new Vector2(px, py);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
