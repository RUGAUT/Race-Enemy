using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cree le panneau d'aide des controles ("?") et gere l'affichage des controles tactiles (WebGL : telephone ou PC).
///   Race Enemy > Build Controls UI (Jeu)         : ajoute dans la scene de jeu ouverte (LV1)
///   Race Enemy > Simuler mobile dans l'editeur   : bascule le test "telephone" dans l'editeur
/// MainMenuProBuilder appelle aussi CreateHelpPanel / CreateHelpButton pour le menu.
/// </summary>
public static class ControlsUIBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/";
    private static TMP_FontAsset font;
    private static Sprite sFrame, sBtnYellow, sBtnBlue, sCircle, sHelp, sGlow;

    // -------------------------------------------------------------- menu editeur

    [MenuItem("Race Enemy/Simuler mobile dans l'editeur")]
    private static void ToggleSimulateMobile()
    {
        bool value = !EditorPrefs.GetBool(PlatformInfo.SimulateMobilePref, false);
        EditorPrefs.SetBool(PlatformInfo.SimulateMobilePref, value);
        Debug.Log("[Controls] Simulation mobile dans l'editeur : " + (value ? "ACTIVEE (controles tactiles visibles)" : "DESACTIVEE (mode PC)"));
    }

    [MenuItem("Race Enemy/Simuler mobile dans l'editeur", true)]
    private static bool ToggleSimulateMobileValidate()
    {
        Menu.SetChecked("Race Enemy/Simuler mobile dans l'editeur", EditorPrefs.GetBool(PlatformInfo.SimulateMobilePref, false));
        return true;
    }

    [MenuItem("Race Enemy/Build Controls UI (Jeu)")]
    public static void BuildForGame()
    {
        Load();
        var canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) { Debug.LogError("[Controls] Canvas introuvable."); return; }
        RectTransform root = canvasGo.GetComponent<RectTransform>();

        Remove(root, "Panel Controles Pro");
        Remove(root, "Bouton Controles Pro"); // ancien bouton place dans le HUD
        var oldVisibility = Object.FindFirstObjectByType<TouchControlsVisibility>();
        if (oldVisibility != null) Object.DestroyImmediate(oldVisibility.gameObject);

        // Le bouton "?" est dans le menu Pause (coin haut droit de la fenetre), pas dans le HUD du niveau.
        var panel = CreateHelpPanel(root);
        var pauseWindow = root.Find("Panel Pause Pro/Fenetre") as RectTransform;
        if (pauseWindow == null)
        {
            Debug.LogWarning("[Controls] 'Panel Pause Pro' introuvable : lance d'abord Race Enemy > Build Pause Panel.");
        }
        else
        {
            Remove(pauseWindow, "Bouton Controles Pro");
            CreateHelpButton(pauseWindow, panel, new Vector2(1, 1), new Vector2(-20, -30), 90);
        }
        panel.transform.SetAsLastSibling(); // au-dessus du menu Pause

        // Controles tactiles : joystick + boutons de tir et de grenade.
        var elements = new System.Collections.Generic.List<RectTransform>();
        foreach (string name in new[] { "Split Joystick (White)", "Button Fire", "Button Grenade" })
        {
            var t = root.Find(name) as RectTransform;
            if (t != null) elements.Add(t); else Debug.LogWarning("[Controls] Element tactile introuvable : " + name);
        }
        var go = new GameObject("Controles Tactiles");
        var vis = go.AddComponent<TouchControlsVisibility>();
        var so = new SerializedObject(vis);
        var arr = so.FindProperty("touchElements");
        arr.arraySize = elements.Count;
        for (int i = 0; i < elements.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        EditorSceneManager.SaveScene(canvasGo.scene);
        SceneLocalizer.Apply();
        Debug.Log("[Controls] Aide des controles + controles tactiles automatiques ajoutes.");
    }

    // ------------------------------------------------------- construction (public)

    /// <summary>Panneau d'aide (cache par defaut) place sous <paramref name="canvasRoot"/>.</summary>
    public static GameObject CreateHelpPanel(RectTransform canvasRoot)
    {
        Load();
        var overlay = Img("Panel Controles Pro", canvasRoot, null, new Color(0, 0, 0, 0.7f));
        Stretch(overlay);

        Color accent = new Color32(0x12, 0xA4, 0xFF, 255);
        var border = Img("Fenetre", overlay, sFrame, accent);
        border.GetComponent<Image>().type = Image.Type.Sliced;
        Place(border, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, -20), new Vector2(1060, 720));
        border.gameObject.AddComponent<PanelPopIn>();
        var inner = Img("Fond", border, sFrame, new Color32(0x0E, 0x2A, 0x5C, 255));
        inner.GetComponent<Image>().type = Image.Type.Sliced;
        Stretch(inner);
        inner.offsetMin = new Vector2(10, 10);
        inner.offsetMax = new Vector2(-10, -10);
        inner.GetComponent<Image>().raycastTarget = false;

        var glow = Img("Lueur", border, sGlow, new Color(accent.r, accent.g, accent.b, 0.5f));
        Place(glow, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 40), new Vector2(1000, 400));
        glow.GetComponent<Image>().raycastTarget = false;

        var banner = Img("Bandeau Titre", border, sBtnBlue, Color.white);
        banner.GetComponent<Image>().type = Image.Type.Sliced;
        banner.GetComponent<Image>().raycastTarget = false;
        Place(banner, 0.5f, 1, 0.5f, 0.5f, new Vector2(0, 20), new Vector2(680, 170));
        var title = Txt("Titre", banner, "CONTRÔLES", 100, Color.white);
        Stretch(title);
        title.offsetMin = new Vector2(10, 14);

        // Deux blocs : un pour PC, un pour telephone (ControlsHelpPanel choisit lequel afficher).
        var pc = Group("Contenu PC", border);
        Row(pc, 0, "Flèches / ZQSD : se déplacer");
        Row(pc, 1, "Ctrl / J : tirer");
        Row(pc, 2, "Espace / G : grenade");
        Row(pc, 3, "Échap / P : pause");

        var touch = Group("Contenu Tactile", border);
        Row(touch, 0, "Joystick (gauche) : se déplacer");
        Row(touch, 1, "Bouton de tir : tirer");
        Row(touch, 2, "Bouton grenade : lancer");
        Row(touch, 3, "Engrenage (haut droite) : pause");

        var panel = overlay.gameObject.AddComponent<ControlsHelpPanel>();
        var so = new SerializedObject(panel);
        so.FindProperty("pcContent").objectReferenceValue = pc.gameObject;
        so.FindProperty("touchContent").objectReferenceValue = touch.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();

        var close = Img("Bouton Fermer", border, sBtnYellow, Color.white);
        close.GetComponent<Image>().type = Image.Type.Sliced;
        Place(close, 0.5f, 0, 0.5f, 0.5f, new Vector2(0, 40), new Vector2(380, 120));
        var closeBtn = close.gameObject.AddComponent<Button>();
        closeBtn.targetGraphic = close.GetComponent<Image>();
        var closeTxt = Txt("Texte", close, "FERMER", 56, Color.white);
        Stretch(closeTxt);
        closeTxt.offsetMin = new Vector2(8, 12);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, panel.Close);

        overlay.gameObject.SetActive(false);
        return overlay.gameObject;
    }

    /// <summary>Bouton rond "?" qui ouvre <paramref name="panel"/>.</summary>
    public static Button CreateHelpButton(RectTransform parent, GameObject panel, Vector2 anchor, Vector2 pos, float size)
    {
        Load();
        var rt = Img("Bouton Controles Pro", parent, sCircle, Color.white);
        Place(rt, anchor.x, anchor.y, anchor.x, anchor.y, pos, new Vector2(size, size));
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = rt.GetComponent<Image>();
        var mark = Txt("Point Interrogation", rt, "?", size * 0.75f, Color.white);
        Stretch(mark);
        mark.offsetMin = new Vector2(0, size * 0.06f);
        UnityEventTools.AddBoolPersistentListener(btn.onClick, panel.SetActive, true);
        return btn;
    }

    // ------------------------------------------------------------------ helpers

    private static RectTransform Group(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        Place(rt, 0.5f, 0.5f, 0.5f, 0.5f, new Vector2(0, 5), new Vector2(980, 420));
        return rt;
    }

    private static void Row(RectTransform group, int index, string text)
    {
        var t = Txt("Ligne " + (index + 1), group, text, 56, Color.white);
        Place(t, 0.5f, 1, 0.5f, 1, new Vector2(0, -index * 100), new Vector2(980, 90));
    }

    private static int FirstEndPanelIndex(RectTransform root)
    {
        int best = root.childCount;
        foreach (string n in new[] { "Panel Game Over Pro", "Panel Win Pro", "Panel Pause Pro" })
        {
            var t = root.Find(n);
            if (t != null) best = Mathf.Min(best, t.GetSiblingIndex());
        }
        return best;
    }

    private static void Remove(RectTransform root, string name)
    {
        var t = root.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject); // uniquement mes propres objets d'une execution precedente
    }

    private static void Load()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LL + "Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        sFrame = S("Sprites/Components/Frame/BasicFrame_Round20.png");
        sBtnYellow = S("Sprites/Components/Button/Button01_225_Yellow.png");
        sBtnBlue = S("Sprites/Components/Button/Button01_225_Blue.png");
        sCircle = S("Sprites/Components/Button/Button_Circle147_Gray.png");
        sHelp = S("Sprites/Components/IconMisc/Icon_PictoIcon_Help.png");
        sGlow = S("Sprites/Demo/Demo_Image/Glow_Cirlce.png");
    }

    private static Sprite S(string rel)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(LL + rel);
        if (s == null) Debug.LogWarning("[Controls] Sprite introuvable : " + rel);
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
}
