using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Ajoute des vehicules dans la scene de jeu ouverte (LV1) en copiant le montage complet de la Muscle
/// (scripts, vie, tir, camera, UI de vie, roues) et en remplacant le modele par un prefab PolygonApocalypse.
/// Ne modifie aucun objet existant : les nouveaux vehicules sont des copies, desactivees par defaut, et
/// ExtraVehicleActivator les active selon le choix du menu.
/// </summary>
public static class VehicleAdder
{
    public const string IconFolder = "Assets/Sprite/Vehicules";
    private const string VehiclePrefabs = "Assets/PolygonApocalypse/Prefabs/Vehicles/";
    private const int TemplateIndex = 0; // Muscle dans GameVehicleActivator (camera et route deja alignees)

    private struct Def
    {
        public string rigName, prefab, icon;
        public float scale;
        public Def(string rigName, string prefab, string icon, float scale)
        { this.rigName = rigName; this.prefab = prefab; this.icon = icon; this.scale = scale; }
    }

    private static readonly Def[] Defs =
    {
        new Def("CarMonster Buggy", "SM_Veh_Buggy_01", "Icone_Buggy", 2.0f),
        new Def("CarMonster Ute", "SM_Veh_Ute_01", "Icone_Ute", 1.39f),
    };

    [MenuItem("Race Enemy/Add Extra Vehicles")]
    public static void Build()
    {
        var activator = Object.FindFirstObjectByType<GameVehicleActivator>(FindObjectsInactive.Include);
        if (activator == null) { Debug.LogError("[Vehicles] GameVehicleActivator introuvable : ouvre la scene de jeu."); return; }

        var list = new SerializedObject(activator).FindProperty("inGameVehicles");
        var template = list.GetArrayElementAtIndex(TemplateIndex).objectReferenceValue as GameObject;
        if (template == null) { Debug.LogError("[Vehicles] Vehicule modele introuvable."); return; }

        // Nettoyage de ma propre version precedente (jamais les objets de l'utilisateur)
        foreach (var d in Defs) DestroyRoot(d.rigName);
        DestroyRoot("Extra Vehicle Activator");

        // Point de depart : celui du premier vehicule (Muscle), meme hauteur que la Muscle (la route generee le repousse vers le haut)
        var first = (list.GetArrayElementAtIndex(0).objectReferenceValue as GameObject).GetComponentInChildren<PlayerControls>(true).transform.position;
        var spawn = new Vector3(first.x, first.y, first.z);

        var rigs = new GameObject[Defs.Length];
        for (int i = 0; i < Defs.Length; i++)
            rigs[i] = BuildRig(template, Defs[i], spawn);

        var go = new GameObject("Extra Vehicle Activator");
        var comp = go.AddComponent<ExtraVehicleActivator>();
        var so = new SerializedObject(comp);
        var arr = so.FindProperty("extraVehicles");
        arr.arraySize = rigs.Length;
        for (int i = 0; i < rigs.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = rigs[i];
        so.FindProperty("firstIndex").intValue = list.arraySize;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(template.scene);
        EditorSceneManager.SaveScene(template.scene);
        Debug.Log("[Vehicles] " + Defs.Length + " vehicules ajoutes (index menu " + list.arraySize + " et suivants).");
    }

    [MenuItem("Race Enemy/Render Vehicle Icons")]
    public static void RenderIcons()
    {
        Directory.CreateDirectory(IconFolder);
        foreach (var d in Defs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehiclePrefabs + d.prefab + ".prefab");
            RenderIcon(prefab, IconFolder + "/" + d.icon + ".png");
        }
        AssetDatabase.Refresh();
    }

    // Supprime uniquement mes propres objets crees par ce script (reconnus par leur nom)
    private static void DestroyRoot(string name)
    {
        var found = new System.Collections.Generic.List<GameObject>();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == name) found.Add(t.gameObject);
        foreach (var go in found) if (go != null) Object.DestroyImmediate(go);
    }

    private static GameObject BuildRig(GameObject template, Def def, Vector3 spawn)
    {
        var rig = Object.Instantiate(template, template.transform.parent); // meme parent que l'original : memes coordonnees
        rig.name = def.rigName;
        rig.SetActive(false);

        var oldModel = FindModel(rig);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VehiclePrefabs + def.prefab + ".prefab");

        var newModel = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig.scene);
        newModel.transform.SetParent(rig.transform, false);
        newModel.transform.localPosition = oldModel.transform.localPosition;
        newModel.transform.localRotation = oldModel.transform.localRotation;
        newModel.transform.localScale = Vector3.one * def.scale;
        newModel.transform.position = spawn;
        newModel.tag = oldModel.tag;
        newModel.layer = oldModel.layer;
        ApplyVehicleMaterial(newModel);

        // Taille du collider (calculee avant d'ajouter les accessoires de tir)
        Bounds local = LocalBounds(newModel, def.scale);

        // 1) Copie des composants de jeu du HotRod vers le nouveau modele (valeurs copiees telles quelles)
        foreach (var c in oldModel.GetComponents<Component>())
        {
            if (c is Transform || c is MeshFilter || c is MeshRenderer) continue;
            var target = newModel.GetComponent(c.GetType());
            if (target == null) target = newModel.AddComponent(c.GetType());
            EditorUtility.CopySerialized(c, target);
        }

        // 2) Les objets ajoutes par l'utilisateur sous le modele (points de tir, VFX, arme) suivent le nouveau modele
        var toMove = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in oldModel.transform)
            if (!child.name.StartsWith("SM_Veh_")) toMove.Add(child); // pieces du HotRod exclues, le reste (tir, VFX, arme) suit
        foreach (var t in toMove) t.SetParent(newModel.transform, false);

        // 3) Collider adapte au modele
        var box = newModel.GetComponent<BoxCollider>();
        if (box != null)
        {
            float height = Mathf.Min(local.size.y * 0.84f, 2.8f / def.scale);
            box.size = new Vector3(local.size.x * 0.9f, height, local.size.z * 0.8f);
            box.center = new Vector3(local.center.x, height * 0.5f, local.center.z);
        }

        // 4) Roues animees
        var wheelsComp = rig.GetComponentInChildren<WheelZRotationClockwise>(true);
        if (wheelsComp != null)
        {
            var found = new System.Collections.Generic.List<Transform>();
            foreach (var t in newModel.GetComponentsInChildren<Transform>(true))
                if (t.name.Contains("_Wheel_") && t.GetComponent<MeshRenderer>() != null) found.Add(t);
            var wso = new SerializedObject(wheelsComp);
            var wArr = wso.FindProperty("wheels");
            wArr.arraySize = found.Count;
            for (int i = 0; i < found.Count; i++) wArr.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
            wso.ApplyModifiedPropertiesWithoutUndo();
        }

        // 5) Camera et UI de vie pointent vers le nouveau modele
        var follow = rig.GetComponentInChildren<CameraFollow>(true);
        if (follow != null)
        {
            var fso = new SerializedObject(follow);
            fso.FindProperty("target").objectReferenceValue = newModel.transform;
            fso.ApplyModifiedPropertiesWithoutUndo();
        }
        var healthUi = rig.GetComponentInChildren<VehicleHealthUI>(true);
        if (healthUi != null)
        {
            var hso = new SerializedObject(healthUi);
            hso.FindProperty("vehicleHealth").objectReferenceValue = newModel.GetComponent<VehicleHealth>();
            hso.ApplyModifiedPropertiesWithoutUndo();
        }

        // 6) L'ancien modele (copie du HotRod) n'est plus utile : c'est ma propre copie, pas l'original
        Object.DestroyImmediate(oldModel);

        newModel.transform.SetSiblingIndex(0);
        return rig;
    }

    private static GameObject FindModel(GameObject rig)
    {
        var pc = rig.GetComponentInChildren<PlayerControls>(true);
        return pc.gameObject;
    }

    /// <summary>Meme materiau URP que tes vehicules existants (le shader du pack n'est pas compatible URP).</summary>
    private static void ApplyVehicleMaterial(GameObject model)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/PolygonApocalypse/Textures/Monster Vehicul texture.mat");
        if (mat == null) { Debug.LogWarning("[Vehicles] Materiau 'Monster Vehicul texture' introuvable."); return; }
        foreach (var r in model.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null && mats[i].shader != null && mats[i].shader.name == "Custom/Test Shader") { mats[i] = mat; changed = true; }
            if (changed) r.sharedMaterials = mats;
        }
    }

    private static Bounds LocalBounds(GameObject model, float scale)
    {
        var renderers = model.GetComponentsInChildren<Renderer>(true);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        var pos = model.transform.position;
        return new Bounds((b.center - pos) / scale, b.size / scale);
    }

    private static void RenderIcon(GameObject prefab, string assetPath)
    {
        // Rendu par une camera temporaire de la scene (pipeline URP), deux fois : fond noir puis fond blanc,
        // pour retrouver la transparence (alpha = 1 - difference entre les deux rendus).
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = new Vector3(0f, -3000f, 0f);
        ApplyVehicleMaterial(go);

        var renderers = go.GetComponentsInChildren<Renderer>();
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);

        var camGo = new GameObject("IconCamera (temporaire)");
        var cam = camGo.AddComponent<Camera>();
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.fieldOfView = 22f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 200f;
        Vector3 dir = new Vector3(1f, 0.3f, 0f).normalized;
        float dist = b.extents.magnitude / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.82f;
        cam.transform.position = b.center + dir * dist;
        cam.transform.rotation = Quaternion.LookRotation(b.center - cam.transform.position, Vector3.up);

        const int W = 800, H = 440;
        var rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;

        Color32[] onBlack = Capture(cam, rt, Color.black, W, H);
        Color32[] onWhite = Capture(cam, rt, Color.white, W, H);

        var pixels = new Color32[W * H];
        for (int i = 0; i < pixels.Length; i++)
        {
            float diff = (onWhite[i].r - onBlack[i].r + onWhite[i].g - onBlack[i].g + onWhite[i].b - onBlack[i].b) / (3f * 255f);
            float alpha = Mathf.Clamp01(1f - diff);
            if (alpha < 0.01f) { pixels[i] = new Color32(0, 0, 0, 0); continue; }
            pixels[i] = new Color32(
                (byte)Mathf.Min(255, onBlack[i].r / alpha),
                (byte)Mathf.Min(255, onBlack[i].g / alpha),
                (byte)Mathf.Min(255, onBlack[i].b / alpha),
                (byte)(alpha * 255f));
        }

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        File.WriteAllBytes(assetPath, tex.EncodeToPNG());

        cam.targetTexture = null;
        rt.Release();
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGo);
        Object.DestroyImmediate(go);

        AssetDatabase.ImportAsset(assetPath);
        var imp = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        imp.textureType = TextureImporterType.Sprite;
        imp.alphaIsTransparency = true;
        imp.SaveAndReimport();
    }

    private static Color32[] Capture(Camera cam, RenderTexture rt, Color background, int w, int h)
    {
        cam.backgroundColor = background;
        cam.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = rt;
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        t.Apply();
        RenderTexture.active = previous;
        var result = t.GetPixels32();
        Object.DestroyImmediate(t);
        return result;
    }
}
