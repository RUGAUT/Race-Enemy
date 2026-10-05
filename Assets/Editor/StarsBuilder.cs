using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Branche les etoiles de victoire (criteres reels) sur "Panel Win Pro" de la scene de jeu ouverte :
/// ajoute une ligne de texte sous chaque etoile et le composant WinStarsDisplay.
/// </summary>
public static class StarsBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/";

    [MenuItem("Race Enemy/Build Win Stars")]
    public static void Build()
    {
        var canvas = GameObject.Find("Canvas");
        var winPanel = canvas != null ? canvas.transform.Find("Panel Win Pro") : null;
        if (winPanel == null)
        {
            Debug.LogError("[Stars] 'Panel Win Pro' introuvable : lance d'abord Race Enemy > Build End Panels.");
            return;
        }
        var window = winPanel.Find("Fenetre") as RectTransform;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LL + "Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        var on = AssetDatabase.LoadAssetAtPath<Sprite>(LL + "Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_On.png");
        var off = AssetDatabase.LoadAssetAtPath<Sprite>(LL + "Sprites/Components/IconMisc/Icon_ImageIcon_StarGrade_l_Off.png");

        // Nettoyage de ma propre version precedente
        var old = winPanel.GetComponent<WinStarsDisplay>();
        if (old != null) Object.DestroyImmediate(old);
        for (int i = 1; i <= 3; i++)
        {
            var oldLabel = window.Find("Etiquette Etoile " + i);
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
        }

        var stars = new Image[3];
        var labels = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            var star = window.Find("Etoile " + (i + 1));
            if (star == null) { Debug.LogError("[Stars] Etoile " + (i + 1) + " introuvable."); return; }
            stars[i] = star.GetComponent<Image>();

            var go = new GameObject("Etiquette Etoile " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(window, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2((i - 1) * 270f, -250f);
            rt.sizeDelta = new Vector2(260, 44);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = 34;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.text = "-";
            labels[i] = t;
        }

        var display = winPanel.gameObject.AddComponent<WinStarsDisplay>();
        var so = new SerializedObject(display);
        var sArr = so.FindProperty("stars");
        sArr.arraySize = 3;
        var lArr = so.FindProperty("labels");
        lArr.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            sArr.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            lArr.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
        }
        so.FindProperty("starOn").objectReferenceValue = on;
        so.FindProperty("starOff").objectReferenceValue = off;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(winPanel.gameObject.scene);
        EditorSceneManager.SaveScene(winPanel.gameObject.scene);
        Debug.Log("[Stars] Etoiles de victoire branchees sur Panel Win Pro.");
    }
}
