using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ajoute "Panel Game Over Pro" et "Panel Win Pro" dans la scene ouverte (LV1),
/// desactive les anciens panels (sans les supprimer) et reconnecte le GameManager.
/// </summary>
public static class EndPanelsBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/";
    private static TMP_FontAsset font;
    private static Sprite sFrame, sBtnGreen, sBtnBlue, sBtnYellow, sBtnRed, sStarOn, sStarOff, sTrophy, sGlow;

    [MenuItem("Race Enemy/Build End Panels (Win + Echec)")]
    public static void Build()
    {
        Load();
        var canvas = GameObject.Find("Canvas");
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (canvas == null || gm == null) { Debug.LogError("[EndPanels] Canvas ou GameManager introuvable."); return; }
        RectTransform root = canvas.GetComponent<RectTransform>();

        RemoveOldPro(root, "Panel Game Over Pro");
        RemoveOldPro(root, "Panel Win Pro");

        var over = BuildPanel(root, "Panel Game Over Pro", false, gm, out TextMeshProUGUI overZ, out TextMeshProUGUI overD);
        var win = BuildPanel(root, "Panel Win Pro", true, gm, out TextMeshProUGUI winZ, out TextMeshProUGUI winD);

        // Anciens panels : desactives, pas supprimes.
        DisableOld(root, "Panel Game Over");
        DisableOld(root, "Panel Win");

        // Reconnexion du GameManager vers les nouveaux panels.
        var so = new SerializedObject(gm);
        so.FindProperty("gameOverPanel").objectReferenceValue = over;
        so.FindProperty("winPanel").objectReferenceValue = win;
        so.FindProperty("finalZombieText").objectReferenceValue = overZ;
        so.FindProperty("finalDistanceText").objectReferenceValue = overD;
        so.FindProperty("winFinalZombieText").objectReferenceValue = winZ;
        so.FindProperty("winFinalDistanceText").objectReferenceValue = winD;
        so.ApplyModifiedPropertiesWithoutUndo();

        over.SetActive(false);
        win.SetActive(false);

        EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
        EditorSceneManager.SaveScene(gm.gameObject.scene);
        Debug.Log("[EndPanels] Panels Win / Echec Pro crees et GameManager reconnecte.");
    }

    private static GameObject BuildPanel(RectTransform root, string name, bool win, GameManager gm, out TextMeshProUGUI zombieText, out TextMeshProUGUI distText)
    {
        var overlay = Img(name, root, null, new Color(0, 0, 0, 0.65f));
        Stretch(overlay);
        overlay.SetAsLastSibling();

        Color accent = win ? new Color32(0xFF, 0xC8, 0x1E, 255) : new Color32(0xFF, 0x3B, 0x3B, 255);

        // Fenetre avec bordure
        var border = Img("Fenetre", overlay, sFrame, accent);
        border.GetComponent<Image>().type = Image.Type.Sliced;
        Place(border, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -20), new Vector2(940, 720));
        border.gameObject.AddComponent<PanelPopIn>();
        var inner = Img("Fond", border, sFrame, new Color32(0x0E, 0x2A, 0x5C, 255));
        inner.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(inner);
        inner.offsetMin = new Vector2(10, 10);
        inner.offsetMax = new Vector2(-10, -10);
        inner.GetComponent<Image>().raycastTarget = false;

        // Lueur derriere le titre
        var glow = Img("Lueur", border, sGlow, new Color(accent.r, accent.g, accent.b, 0.55f));
        Place(glow, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 40), new Vector2(900, 420));
        glow.GetComponent<Image>().raycastTarget = false;

        // Bandeau titre
        var banner = Img("Bandeau Titre", border, win ? sBtnYellow : sBtnRed, Color.white);
        banner.GetComponent<Image>().type = Image.Type.Sliced;
        banner.GetComponent<Image>().raycastTarget = false;
        Place(banner, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 20), new Vector2(720, 170));
        var title = Txt("Titre", banner, win ? "VICTOIRE !" : "ÉCHEC", 110, Color.white);
        Stretch(title);
        title.offsetMin = new Vector2(10, 14);

        // Etoiles
        for (int i = 0; i < 3; i++)
        {
            var star = Img("Etoile " + (i + 1), border, win ? sStarOn : sStarOff, Color.white);
            float size = i == 1 ? 170 : 130;
            Place(star, 0.5f, 1, 0.5f, 0.5f, new Vector2((i - 1) * 190, -170 + (i == 1 ? 18 : 0)), new Vector2(size, size));
            star.GetComponent<Image>().preserveAspect = true;
            star.GetComponent<Image>().raycastTarget = false;
            var pop = star.gameObject.AddComponent<PanelPopIn>();
            SetFloat(pop, "delay", 0.35f + 0.2f * i);
            SetFloat(pop, "startScale", 0f);
        }

        // Carte stat zombies
        var card = Img("Carte Zombies", border, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        card.GetComponent<Image>().type = Image.Type.Sliced;
        Place(card, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -40), new Vector2(720, 120));
        card.GetComponent<Image>().raycastTarget = false;
        zombieText = Txt("Texte Zombies", card, "Zombies Tués: 0", 64, Color.white).GetComponent<TextMeshProUGUI>();
        Stretch(zombieText.rectTransform);
        zombieText.rectTransform.offsetMin = new Vector2(0, 8);

        // Distance (masquee comme dans l'ancien panel, mais toujours reliee au GameManager)
        var dCard = Img("Carte Distance", border, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        dCard.GetComponent<Image>().type = Image.Type.Sliced;
        Place(dCard, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -170), new Vector2(720, 100));
        distText = Txt("Texte Distance", dCard, "Distance: 0m", 56, Color.white).GetComponent<TextMeshProUGUI>();
        Stretch(distText.rectTransform);
        dCard.gameObject.SetActive(false);

        // Record
        var rec = Img("Ligne Record", border, null, new Color(0, 0, 0, 0));
        Place(rec, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -135), new Vector2(720, 80));
        rec.GetComponent<Image>().raycastTarget = false;
        var trophy = Img("Trophee", rec, sTrophy, Color.white);
        Place(trophy, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(-170, 0), new Vector2(70, 70));
        trophy.GetComponent<Image>().preserveAspect = true;
        trophy.GetComponent<Image>().raycastTarget = false;
        var recLbl = Txt("Texte Record", rec, "RECORD :", 46, new Color32(0x7F, 0xD0, 0xFF, 255));
        Place(recLbl, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(10, 0), new Vector2(260, 70));
        var recVal = Txt("Valeur Record", rec, "0", 52, Color.white);
        Place(recVal, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(190, 0), new Vector2(180, 70));
        var stats = rec.gameObject.AddComponent<MenuStatsDisplay>();
        var sso = new SerializedObject(stats);
        sso.FindProperty("zombieRecordText").objectReferenceValue = recVal.GetComponent<TextMeshProUGUI>();
        sso.ApplyModifiedPropertiesWithoutUndo();

        // Boutons
        var menuBtn = Btn("Bouton Menu", border, sBtnBlue, "MENU", new Vector2(-190, 55), new Vector2(340, 120));
        var retryBtn = Btn("Bouton Rejouer", border, sBtnGreen, "REJOUER", new Vector2(190, 55), new Vector2(340, 120));
        UnityEventTools.AddStringPersistentListener(menuBtn.onClick, gm.OnLoadSceneButtonClick, "MainMenu_Pro");
        UnityEventTools.AddPersistentListener(retryBtn.onClick, gm.OnRestartButtonClick);

        return overlay.gameObject;
    }

    [MenuItem("Race Enemy/Build Pause Panel")]
    public static void BuildPause()
    {
        Load();
        var canvas = GameObject.Find("Canvas");
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (canvas == null || gm == null) { Debug.LogError("[PausePanel] Canvas ou GameManager introuvable."); return; }
        RectTransform root = canvas.GetComponent<RectTransform>();
        RemoveOldPro(root, "Panel Pause Pro");

        var overlay = Img("Panel Pause Pro", root, null, new Color(0, 0, 0, 0.65f));
        Stretch(overlay);
        overlay.SetAsLastSibling();

        Color accent = new Color32(0x12, 0xA4, 0xFF, 255);
        var border = Img("Fenetre", overlay, sFrame, accent);
        border.GetComponent<Image>().type = Image.Type.Sliced;
        Place(border, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -20), new Vector2(860, 760));
        border.gameObject.AddComponent<PanelPopIn>();
        var inner = Img("Fond", border, sFrame, new Color32(0x0E, 0x2A, 0x5C, 255));
        inner.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(inner);
        inner.offsetMin = new Vector2(10, 10);
        inner.offsetMax = new Vector2(-10, -10);
        inner.GetComponent<Image>().raycastTarget = false;

        var glow = Img("Lueur", border, sGlow, new Color(accent.r, accent.g, accent.b, 0.5f));
        Place(glow, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 40), new Vector2(860, 400));
        glow.GetComponent<Image>().raycastTarget = false;

        var banner = Img("Bandeau Titre", border, sBtnBlue, Color.white);
        banner.GetComponent<Image>().type = Image.Type.Sliced;
        banner.GetComponent<Image>().raycastTarget = false;
        Place(banner, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 20), new Vector2(620, 170));
        var title = Txt("Titre", banner, "PAUSE", 110, Color.white);
        Stretch(title);
        title.offsetMin = new Vector2(10, 14);

        // Volume
        var volLbl = Txt("Texte Volume", border, "VOLUME", 48, new Color32(0x7F, 0xD0, 0xFF, 255));
        Place(volLbl, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, 150), new Vector2(500, 60));
        var sliderGo = new GameObject("Slider Volume", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(border, false);
        var srt = sliderGo.GetComponent<RectTransform>();
        Place(srt, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, 85), new Vector2(560, 46));
        var back = Img("Fond", srt, sFrame, new Color32(0x08, 0x18, 0x3A, 255));
        back.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(back);
        var fa = new GameObject("Zone Remplissage", typeof(RectTransform)).GetComponent<RectTransform>();
        fa.SetParent(srt, false);
        Stretch(fa);
        fa.offsetMin = new Vector2(7, 7);
        fa.offsetMax = new Vector2(-7, -7);
        var fill = Img("Remplissage", fa, sFrame, new Color32(0xFF, 0xC8, 0x1E, 255));
        fill.GetComponent<Image>().type = Image.Type.Sliced;
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0, 1);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        var slider = sliderGo.GetComponent<Slider>();
        slider.fillRect = fill;
        slider.targetGraphic = back.GetComponent<Image>();
        slider.transition = Selectable.Transition.None;
        slider.value = 1;
        sliderGo.AddComponent<MenuVolumeSlider>();

        // Boutons
        var resume = Btn("Bouton Reprendre", border, sBtnGreen, "REPRENDRE", new Vector2(0, 290), new Vector2(520, 130));
        var retry = Btn("Bouton Rejouer", border, sBtnYellow, "REJOUER", new Vector2(-200, 110), new Vector2(340, 120));
        var menu = Btn("Bouton Menu", border, sBtnBlue, "MENU", new Vector2(200, 110), new Vector2(340, 120));
        UnityEventTools.AddPersistentListener(resume.onClick, gm.OnResumeButtonClick);
        UnityEventTools.AddPersistentListener(retry.onClick, gm.OnRestartButtonClick);
        UnityEventTools.AddStringPersistentListener(menu.onClick, gm.OnLoadSceneButtonClick, "MainMenu_Pro");

        DisableOld(root, "Panel Pause");
        var so = new SerializedObject(gm);
        so.FindProperty("pauseMenuPanel").objectReferenceValue = overlay.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        overlay.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
        EditorSceneManager.SaveScene(gm.gameObject.scene);
        Debug.Log("[PausePanel] Panel Pause Pro cree et GameManager reconnecte.");
    }

    private static Button Btn(string name, RectTransform parent, Sprite sprite, string label, Vector2 pos, Vector2 size)
    {
        var rt = Img(name, parent, sprite, Color.white);
        rt.GetComponent<Image>().type = Image.Type.Sliced;
        Place(rt, 0.5f, 0, 0.5f, 0.5f, pos, size);
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = rt.GetComponent<Image>();
        var txt = Txt("Texte", rt, label, 56, Color.white);
        Stretch(txt);
        txt.offsetMin = new Vector2(8, 12);
        return btn;
    }

    private static void RemoveOldPro(RectTransform root, string name)
    {
        var t = root.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject); // uniquement mes propres panels Pro d'une execution precedente
    }

    private static void DisableOld(RectTransform root, string name)
    {
        var t = root.Find(name);
        if (t != null) t.gameObject.SetActive(false);
    }

    private static void Load()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LL + "Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        sFrame = S("Sprites/Components/Frame/BasicFrame_Round20.png");
        sBtnGreen = S("Sprites/Components/Button/Button01_225_Green.png");
        sBtnBlue = S("Sprites/Components/Button/Button01_225_Blue.png");
        sBtnYellow = S("Sprites/Components/Button/Button01_225_Yellow.png");
        sBtnRed = S("Sprites/Components/Button/Button01_225_Red.png");
        sStarOn = S("Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_On.png");
        sStarOff = S("Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_Off.png");
        sTrophy = S("Sprites/Components/IconMisc/Icon_ImageIcon_Trophy_m.png");
        sGlow = S("Sprites/Demo/Demo_Image/Glow_Cirlce.png");
    }

    private static Sprite S(string rel)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(LL + rel);
        if (s == null) Debug.LogWarning("[EndPanels] Sprite introuvable : " + rel);
        return s;
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

    private static void SetFloat(Object target, string field, float v)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).floatValue = v;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
