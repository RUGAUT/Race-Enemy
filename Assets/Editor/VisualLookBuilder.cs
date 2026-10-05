using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Cree le profil d'effets "cartoon colore" (Assets/Rendering/RaceEnemyLook.asset) et l'objet "Rendu Visuel"
/// (Volume global + VisualEnhancer) dans la scene de jeu ouverte. Effets legers, adaptes au mobile / WebGL.
/// </summary>
public static class VisualLookBuilder
{
    private const string ProfilePath = "Assets/Rendering/RaceEnemyLook.asset";
    private const string ObjectName = "Rendu Visuel";

    [MenuItem("Race Enemy/Build Visual Look")]
    public static void Build()
    {
        var profile = CreateProfile();

        // Ma propre version precedente de l'objet
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t != null && t.name == ObjectName) { Object.DestroyImmediate(t.gameObject); break; }

        var go = new GameObject(ObjectName);
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
        go.AddComponent<VisualEnhancer>();

        EditorSceneManager.MarkSceneDirty(go.scene);
        EditorSceneManager.SaveScene(go.scene);
        Debug.Log("[Look] Rendu visuel ajoute a la scene " + go.scene.name);
    }

    private static VolumeProfile CreateProfile()
    {
        Directory.CreateDirectory("Assets/Rendering");
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath) != null)
            AssetDatabase.DeleteAsset(ProfilePath); // mon propre asset, recree a l'identique

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, ProfilePath);

        var tone = Add<Tonemapping>(profile);
        tone.mode.Override(TonemappingMode.Neutral);

        var colors = Add<ColorAdjustments>(profile);
        colors.postExposure.Override(0.5f);
        colors.contrast.Override(15f);
        colors.saturation.Override(35f);

        var white = Add<WhiteBalance>(profile);
        white.temperature.Override(8f);

        var bloom = Add<Bloom>(profile);
        bloom.threshold.Override(0.8f);
        bloom.intensity.Override(0.45f);
        bloom.scatter.Override(0.6f);

        var vignette = Add<Vignette>(profile);
        vignette.intensity.Override(0.25f);
        vignette.smoothness.Override(0.45f);

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    private static T Add<T>(VolumeProfile profile) where T : VolumeComponent
    {
        var component = profile.Add<T>(true);
        AssetDatabase.AddObjectToAsset(component, profile);
        return component;
    }
}
