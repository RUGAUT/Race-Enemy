using UnityEngine;

/// <summary>
/// Reglages du bouclier Force Field (asset Resources/ShieldSettings, cree par Race Enemy > Setup Vehicle Shield).
/// </summary>
public class ShieldSettings : ScriptableObject
{
    public Material material;
    public TMPro.TMP_FontAsset font;
    [Tooltip("Marge autour du vehicule (1 = taille exacte du collider).")]
    public float padding = 1.5f;
    [Tooltip("Petite pulsation du bouclier (0 = aucune).")]
    public float pulseAmount = 0.04f;
    public float pulseSpeed = 4f;

    [Header("Effets des capacites")]
    public GameObject shockwaveVfx;
    public GameObject healVfx;
    public GameObject turboVfx;
}
