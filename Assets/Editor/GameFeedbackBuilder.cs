using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Ajoute l'objet "Retours de Jeu" (script GameFeedback) dans la scene de jeu ouverte.
/// </summary>
public static class GameFeedbackBuilder
{
    private const string LL = "Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/";

    [MenuItem("Race Enemy/Build Game Feedback")]
    public static void Build()
    {
        var existing = Object.FindFirstObjectByType<GameFeedback>();
        if (existing != null) Object.DestroyImmediate(existing.gameObject); // uniquement mon propre objet d'une execution precedente

        var go = new GameObject("Retours de Jeu");
        var fb = go.AddComponent<GameFeedback>();

        var so = new SerializedObject(fb);
        so.FindProperty("font").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LL + "Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        so.FindProperty("bannerSprite").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Sprite>(LL + "Sprites/Components/Button/Button01_225_Red.png");
        so.ApplyModifiedPropertiesWithoutUndo();

        var scene = go.scene;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GameFeedback] Objet 'Retours de Jeu' ajoute a la scene " + scene.name);
    }
}
