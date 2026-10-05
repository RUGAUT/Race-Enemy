using UnityEngine;

/// <summary>
/// Bouclier visuel Force Field autour du vehicule du joueur, affiche pendant que la barriere est active
/// (VehicleHealth.HasActiveBarrier). La taille et la position viennent du collider du vehicule : le bouclier
/// est donc toujours colle au vehicule, quel que soit le modele. L'ancien visuel de barriere est masque ;
/// la logique (invulnerabilite, obstacles detruits) reste celle de VehicleHealth, non modifiee.
/// Ajoute automatiquement au vehicule actif au lancement de la scene.
/// </summary>
public class VehicleShield : MonoBehaviour
{
    private ShieldSettings settings;
    private VehicleHealth health;
    private BoxCollider box;
    private GameObject shield;
    private bool shown;

    public static void Setup()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<VehicleHealth>() == null) return;
        if (player.GetComponent<VehicleShield>() == null) player.AddComponent<VehicleShield>();
    }

    private void Awake()
    {
        settings = Resources.Load<ShieldSettings>("ShieldSettings");
        health = GetComponent<VehicleHealth>();
        box = GetComponent<BoxCollider>();

        if (settings == null || settings.material == null || box == null)
        {
            enabled = false;
            return;
        }

        shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shield.name = "Bouclier (Force Field)";
        Destroy(shield.GetComponent<Collider>());
        var renderer = shield.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = settings.material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        shield.SetActive(false);
    }

    private void LateUpdate()
    {
        bool active = health != null && health.HasActiveBarrier;
        if (active != shown)
        {
            shown = active;
            shield.SetActive(active);
        }
        if (!active) return;

        HideOldBarrierVisual();

        var bounds = box.bounds;
        float pulse = 1f + Mathf.Sin(Time.time * settings.pulseSpeed) * settings.pulseAmount;
        // Plus haut que le collider (qui est bas) : une vraie bulle qui englobe le toit et les armes
        var size = bounds.size * settings.padding;
        size.y = Mathf.Max(size.y * 1.3f, size.z * 0.62f);
        shield.transform.position = bounds.center + Vector3.up * (size.y * 0.1f);
        shield.transform.rotation = Quaternion.identity;
        shield.transform.localScale = size * pulse;
    }

    // L'ancienne barriere est instanciee comme enfant du vehicule : on masque son rendu, pas son objet.
    private void HideOldBarrierVisual()
    {
        foreach (Transform child in transform)
        {
            if (!child.name.EndsWith("(Clone)")) continue;
            foreach (var r in child.GetComponentsInChildren<MeshRenderer>())
                if (r.enabled) r.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (shield != null) Destroy(shield);
    }
}
