using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Cree Assets/Resources/ShieldSettings.asset (materiau Force Field du bouclier).</summary>
public static class ShieldBuilder
{
    private const string AssetPath = "Assets/Resources/ShieldSettings.asset";
    private const string MaterialPath = "Assets/Ultimate 10 Plus Shaders/Materials/Unique/Force Field.mat";

    [MenuItem("Race Enemy/Setup Vehicle Shield")]
    public static void Build()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null) { Debug.LogError("[Shield] Materiau Force Field introuvable : " + MaterialPath); return; }

        Directory.CreateDirectory("Assets/Resources");
        var settings = AssetDatabase.LoadAssetAtPath<ShieldSettings>(AssetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<ShieldSettings>();
            AssetDatabase.CreateAsset(settings, AssetPath);
        }
        settings.material = material;
        settings.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Layer Lab/GUI Pro-CasualGame/ResourcesData/Fonts/LilitaOne-Regular Outline_Extended ASCII_120 SDF.asset");
        settings.shockwaveVfx = FindPrefab("CFX4 Disruptive Force");
        settings.healVfx = FindPrefab("CFXR2 Shiny Item (Loop)");
        settings.turboVfx = FindPrefab("CFXR Magic Poof");
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("[Shield] Reglages du bouclier prets : " + AssetPath);
    }

    private static GameObject FindPrefab(string name)
    {
        foreach (var guid in AssetDatabase.FindAssets(name + " t:Prefab"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == name) return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        Debug.LogWarning("[Shield] Effet introuvable : " + name);
        return null;
    }
}
