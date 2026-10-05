using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ambiance visuelle "cartoon colore" appliquee au lancement de la partie : post-traitement sur les cameras,
/// soleil chaud, lumiere ambiante douce, brouillard clair. Rien n'est enregistre dans tes objets ni dans
/// l'asset URP : les changements disparaissent a l'arret du jeu. Desactive ce composant pour revenir
/// au rendu d'origine.
/// </summary>
public class VisualEnhancer : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private bool enablePostProcessing = true;
    [SerializeField] private bool smoothEdges = true;

    [Header("Lumiere")]
    [SerializeField] private bool applyLighting = true;
    [SerializeField] private Color sunColor = new Color(1f, 0.95f, 0.84f);
    [SerializeField] private float sunIntensity = 1.5f;
    [SerializeField, Range(0f, 1f)] private float shadowStrength = 0.75f;
    [SerializeField] private Color skyAmbient = new Color(0.72f, 0.85f, 1f);
    [SerializeField] private Color equatorAmbient = new Color(0.88f, 0.9f, 0.84f);
    [SerializeField] private Color groundAmbient = new Color(0.58f, 0.53f, 0.48f);

    [Header("Brouillard")]
    [SerializeField] private bool applyFog = true;
    [SerializeField] private Color fogColor = new Color(0.78f, 0.88f, 0.98f);
    [SerializeField] private float fogDensity = 0.004f;

    private void Awake()
    {
        if (enablePostProcessing || smoothEdges) SetupCameras();
        if (applyLighting) SetupLighting();
        if (applyFog) SetupFog();
    }

    private void SetupCameras()
    {
        // Tous les vehicules ont leur propre camera (certaines sont inactives au depart)
        foreach (var cam in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data == null) continue;
            if (enablePostProcessing) data.renderPostProcessing = true;
            if (smoothEdges)
            {
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.Low;
            }
        }
    }

    private void SetupLighting()
    {
        var sun = RenderSettings.sun;
        if (sun != null)
        {
            sun.color = sunColor;
            sun.intensity = sunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = shadowStrength;
        }

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyAmbient;
        RenderSettings.ambientEquatorColor = equatorAmbient;
        RenderSettings.ambientGroundColor = groundAmbient;
    }

    private void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }
}
