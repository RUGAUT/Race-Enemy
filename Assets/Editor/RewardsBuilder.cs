using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Ajoute RunRewards (gain de pieces) sur les panels de victoire et de defaite de la scene de jeu ouverte.</summary>
public static class RewardsBuilder
{
    private const string FontPath = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset";

    private const string CoinPath = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Sprites/Components/Icon_ItemIcons/128/Icon_Golds.png";

    [MenuItem("Race Enemy/Add Run Rewards")]
    public static void Build()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("[Rewards] Canvas introuvable : ouvre la scene de jeu."); return; }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Setup(canvas.transform.Find("Panel Win Pro"), true, font);
        Setup(canvas.transform.Find("Panel Game Over Pro"), false, font);

        var scene = canvas.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Rewards] Gain de pieces branche sur les panels de fin.");
    }

    private static void Setup(Transform panel, bool victory, TMP_FontAsset font)
    {
        if (panel == null) { Debug.LogWarning("[Rewards] Panel introuvable."); return; }

        var old = panel.GetComponent<RunRewards>();
        if (old != null) Object.DestroyImmediate(old); // ma propre version precedente

        var rewards = panel.gameObject.AddComponent<RunRewards>();
        var so = new SerializedObject(rewards);
        so.FindProperty("victory").boolValue = victory;
        so.FindProperty("font").objectReferenceValue = font;
        so.FindProperty("coinIcon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(CoinPath);
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
