using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ajoute le bouton de capacite ("Button Capacite") dans le Canvas de la scene de jeu ouverte, au-dessus du bouton
/// de grenade, et l'enregistre dans TouchControlsVisibility (visible sur telephone, cache sur PC).
/// </summary>
public static class AbilityBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/";
    private const string ButtonName = "Button Capacite";

    [MenuItem("Race Enemy/Build Ability Button")]
    public static void Build()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("[Ability] Canvas introuvable : ouvre la scene de jeu."); return; }
        var canvasRt = canvas.transform as RectTransform;

        // Ma propre version precedente
        var old = canvasRt.Find(ButtonName);
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var grenade = canvasRt.Find("Button Grenade") as RectTransform;
        var basePos = grenade != null ? grenade.anchoredPosition : new Vector2(779f, -268f);

        // Bouton rond
        var root = NewImage(ButtonName, canvasRt, Load("Button/Button_Circle147_Navy.png"), Color.white);
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(210f, 210f);
        root.anchoredPosition = new Vector2(basePos.x, basePos.y + 270f);
        var rootImage = root.GetComponent<Image>();
        var button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = rootImage;

        // Icone de la capacite
        var icon = NewImage("Icone", root, null, Color.white);
        icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
        icon.sizeDelta = new Vector2(120f, 120f);
        icon.GetComponent<Image>().raycastTarget = false;
        icon.GetComponent<Image>().preserveAspect = true;

        // Jauge de recharge (voile sombre qui se vide en cercle)
        var fill = NewImage("Recharge", root, Load("Button/Button_Circle147_Navy.png"), new Color(0f, 0f, 0f, 0.62f));
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        var fillImage = fill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = false;
        fillImage.fillAmount = 0f;
        fillImage.raycastTarget = false;
        fillImage.enabled = false;

        // Composant + icones (ordre : SuperShot, Turbo, Heal, Shield, Shockwave)
        var ability = root.gameObject.AddComponent<AbilityButton>();
        var so = new SerializedObject(ability);
        so.FindProperty("icon").objectReferenceValue = icon.GetComponent<Image>();
        so.FindProperty("cooldownFill").objectReferenceValue = fillImage;
        var icons = so.FindProperty("icons");
        string[] names = { "Icon_Sword", "Icon_Bolt", "Icon_Heart", "Icon_Shield", "Icon_Star" };
        icons.arraySize = names.Length;
        for (int i = 0; i < names.Length; i++)
            icons.GetArrayElementAtIndex(i).objectReferenceValue = Load("Icon_ItemIcons/128/" + names[i] + ".png");
        so.ApplyModifiedPropertiesWithoutUndo();

        // Icone visible dans l'editeur : celle de la premiere capacite (remplacee au lancement selon le vehicule)
        icon.GetComponent<Image>().sprite = Load("Icon_ItemIcons/128/" + names[0] + ".png");

        UnityEventTools.AddVoidPersistentListener(button.onClick, ability.Use);

        RegisterTouchElement(root);

        EditorSceneManager.MarkSceneDirty(canvas.scene);
        EditorSceneManager.SaveScene(canvas.scene);
        Debug.Log("[Ability] Bouton de capacite ajoute.");
    }

    private static void RegisterTouchElement(RectTransform element)
    {
        var visibility = Object.FindFirstObjectByType<TouchControlsVisibility>(FindObjectsInactive.Include);
        if (visibility == null) return;

        var so = new SerializedObject(visibility);
        var array = so.FindProperty("touchElements");
        for (int i = array.arraySize - 1; i >= 0; i--)
            if (array.GetArrayElementAtIndex(i).objectReferenceValue == null) array.DeleteArrayElementAtIndex(i); // reste de ma version precedente
        for (int i = 0; i < array.arraySize; i++)
            if (array.GetArrayElementAtIndex(i).objectReferenceValue == element) return;

        array.arraySize++;
        array.GetArrayElementAtIndex(array.arraySize - 1).objectReferenceValue = element;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform NewImage(string name, RectTransform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return go.GetComponent<RectTransform>();
    }

    private static Sprite Load(string relative)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LL + relative);
        if (sprite == null) Debug.LogWarning("[Ability] Sprite introuvable : " + relative);
        return sprite;
    }
}
