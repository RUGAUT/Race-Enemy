using System.Collections;
using System.Reflection;
using UnityEngine;

public enum AbilityType { SuperShot, Turbo, Heal, Shield, Shockwave }

/// <summary>Capacite de chaque vehicule (index = ordre du menu : 0 Muscle, 1 HotRod, 2 CarWhite, 3 Buggy, 4 Ute).</summary>
public static class AbilityCatalog
{
    public struct Info
    {
        public AbilityType type;
        public float cooldown;
        public float duration;
        public Info(AbilityType type, float cooldown, float duration) { this.type = type; this.cooldown = cooldown; this.duration = duration; }
    }

    private static readonly Info[] Abilities =
    {
        new Info(AbilityType.SuperShot, 12f, 5f),
        new Info(AbilityType.Shield,    15f, 5f),
        new Info(AbilityType.Heal,      25f, 0f),
        new Info(AbilityType.Turbo,     10f, 2f),
        new Info(AbilityType.Shockwave, 14f, 0f),
    };

    public static Info Get(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Abilities.Length) return Abilities[0];
        return Abilities[vehicleIndex];
    }
}

/// <summary>
/// Capacite speciale du vehicule du joueur (ajoutee automatiquement au lancement de la scene).
/// Aucun script existant n'est modifie : tout passe par leurs methodes publiques.
///   Super tir   : tir triple + cadence doublee.
///   Turbo       : le vehicule fonce, est invulnerable et ecrase les zombies (a l'arret pendant un boss : il reste
///                 en place mais reste invulnerable et ecrase les zombies au contact).
///   Soin        : rend 30 % de vie.
///   Bouclier    : barriere Force Field (VehicleHealth.ActivateBarrier).
///   Onde de choc: repousse et fige brievement tous les zombies autour. Le boss n'est jamais touche.
/// </summary>
public class VehicleAbility : MonoBehaviour
{
    public static VehicleAbility Instance { get; private set; }

    [SerializeField] private float shockwaveRadius = 22f;
    [SerializeField] private float shockwavePushDistance = 14f;
    [SerializeField] private float shockwaveStun = 1.2f;
    [SerializeField] private float turboSpeedMultiplier = 2.2f;
    [SerializeField] private float healFraction = 0.3f;

    private AbilityCatalog.Info info;
    private VehicleHealth health;
    private CarLaneController lane;
    private BoxCollider box;
    private VehicleAttack attack;
    private ShieldSettings settings;
    private float readyTime;

    public AbilityType Type => info.type;
    public float CooldownDuration => info.cooldown;
    public bool Ready => Time.time >= readyTime;

    /// <summary>0 = pret, 1 = vient d'etre utilise (sert a la jauge du bouton).</summary>
    public float CooldownProgress => Ready ? 0f : Mathf.Clamp01((readyTime - Time.time) / info.cooldown);

    public static void Setup()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || player.GetComponent<VehicleHealth>() == null) return;
        if (player.GetComponent<VehicleAbility>() == null) player.AddComponent<VehicleAbility>();
    }

    private void Awake()
    {
        Instance = this;
        info = AbilityCatalog.Get(PlayerPrefs.GetInt("SelectedVehicleIndex", 0));
        health = GetComponent<VehicleHealth>();
        lane = GetComponent<CarLaneController>();
        box = GetComponent<BoxCollider>();
        attack = GetComponent<VehicleAttack>();
        settings = Resources.Load<ShieldSettings>("ShieldSettings");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Appelee par le bouton a l'ecran et par la touche clavier.</summary>
    public static void Use()
    {
        if (Instance != null) Instance.TryUse();
    }

    public bool TryUse()
    {
        if (Time.timeScale <= 0f || !Ready || health == null || health.CurrentHealth <= 0) return false;

        switch (info.type)
        {
            case AbilityType.SuperShot: StartCoroutine(SuperShot()); break;
            case AbilityType.Turbo: StartCoroutine(Turbo()); break;
            case AbilityType.Heal: Heal(); break;
            case AbilityType.Shield:
                if (!health.ActivateBarrier(info.duration)) return false;
                break;
            case AbilityType.Shockwave: Shockwave(); break;
        }

        readyTime = Time.time + info.cooldown;
        AbilityFeedback.Show(Loc.T("ability_" + info.type.ToString().ToLowerInvariant()), FeedbackColor(info.type));
        StartCoroutine(AnnounceReady());
        return true;
    }

    private IEnumerator AnnounceReady()
    {
        float target = readyTime;
        while (Time.time < target) yield return null;
        // Seulement si la capacite n'a pas ete reutilisee entre-temps
        if (Mathf.Approximately(target, readyTime)) AbilityFeedback.Show(Loc.T("ability_ready"), Color.white);
    }

    private static Color FeedbackColor(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.SuperShot: return new Color32(0xFF, 0xC8, 0x1E, 255);
            case AbilityType.Turbo: return new Color32(0xFF, 0x7A, 0x1E, 255);
            case AbilityType.Heal: return new Color32(0x4C, 0xE0, 0x5A, 255);
            case AbilityType.Shield: return new Color32(0x4C, 0xC8, 0xFF, 255);
            default: return new Color32(0xB0, 0x7A, 0xFF, 255);
        }
    }

    // ------------------------------------------------------------------ Super tir

    private IEnumerator SuperShot()
    {
        VehicleAttack.ActivateTripleShot(info.duration);
        float oldFire = GetFloat(attack, "fireRate");
        float oldAuto = GetFloat(attack, "autoFireRate");
        SetFloat(attack, "fireRate", oldFire * 0.5f);
        SetFloat(attack, "autoFireRate", oldAuto * 0.5f);

        yield return new WaitForSeconds(info.duration);

        SetFloat(attack, "fireRate", oldFire);
        SetFloat(attack, "autoFireRate", oldAuto);
    }

    // ------------------------------------------------------------------ Turbo

    private IEnumerator Turbo()
    {
        float baseSpeed = lane != null ? lane.forwardSpeed : 0f;
        health.ActivateInvulnerability(info.duration);
        Spawn(settings != null ? settings.turboVfx : null, transform.position, 2f);

        var cam = Camera.main;
        float baseFov = cam != null ? cam.fieldOfView : 60f;

        float elapsed = 0f;
        while (elapsed < info.duration)
        {
            elapsed += Time.deltaTime;

            // A l'arret pour un boss, on ne bouge pas : seule l'invulnerabilite et l'ecrasement restent actifs.
            if (lane != null) lane.forwardSpeed = lane.isStoppedForBoss ? baseSpeed : baseSpeed * turboSpeedMultiplier;

            if (cam != null)
            {
                float k = Mathf.Sin(Mathf.Clamp01(elapsed / info.duration) * Mathf.PI); // monte puis redescend
                cam.fieldOfView = baseFov + 10f * k;
            }

            CrushZombies();
            yield return null;
        }

        if (lane != null) lane.forwardSpeed = baseSpeed;
        if (cam != null) cam.fieldOfView = baseFov;
    }

    private void CrushZombies()
    {
        if (box == null) return;
        var b = box.bounds;
        var half = b.extents + new Vector3(0.8f, 1f, 1.8f);
        foreach (var col in Physics.OverlapBox(b.center, half, Quaternion.identity, ~0, QueryTriggerInteraction.Collide))
        {
            var zombieHealth = col.GetComponentInParent<Health>();
            if (zombieHealth == null || IsBoss(zombieHealth.gameObject)) continue;
            zombieHealth.TakeDamage(99999);
        }
    }

    // ------------------------------------------------------------------ Soin

    private void Heal()
    {
        health.RestoreHealth(Mathf.RoundToInt(health.MaxHealth * healFraction));
        Spawn(settings != null ? settings.healVfx : null, transform.position, 2.5f);
    }

    // ------------------------------------------------------------------ Onde de choc

    private void Shockwave()
    {
        Spawn(settings != null ? settings.shockwaveVfx : null, box != null ? box.bounds.center : transform.position, 3f);

        var cam = Camera.main;
        if (cam != null)
        {
            var follow = cam.GetComponent<CameraFollow>();
            if (follow != null) follow.TriggerShake(0.45f, 0.6f);
        }

        Vector3 origin = transform.position;
        foreach (var zombieHealth in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (zombieHealth == null || IsBoss(zombieHealth.gameObject)) continue;

            Vector3 offset = zombieHealth.transform.position - origin;
            offset.y = 0f;
            if (offset.sqrMagnitude > shockwaveRadius * shockwaveRadius) continue;

            Vector3 dir = offset.sqrMagnitude < 0.01f ? Vector3.forward : offset.normalized;
            StartCoroutine(PushBack(zombieHealth.gameObject, dir));
        }
    }

    // Le zombie glisse en arriere et est fige un instant (ses scripts de deplacement sont coupes puis remis).
    private IEnumerator PushBack(GameObject zombie, Vector3 direction)
    {
        var frozen = new System.Collections.Generic.List<Behaviour>();
        foreach (var behaviour in new Behaviour[] { zombie.GetComponent<Zombie>(), zombie.GetComponent<KamikazeZombie>() })
            if (behaviour != null && behaviour.enabled) { behaviour.enabled = false; frozen.Add(behaviour); }

        float t = 0f;
        const float slide = 0.45f;
        while (t < shockwaveStun && zombie != null)
        {
            if (t < slide)
            {
                float step = Time.deltaTime / slide;
                zombie.transform.position += direction * shockwavePushDistance * step * (1.5f - t / slide);
            }
            t += Time.deltaTime;
            yield return null;
        }

        foreach (var behaviour in frozen)
            if (behaviour != null) behaviour.enabled = true;
    }

    // ------------------------------------------------------------------ Utilitaires

    // Le boss (et ses obstacles) ne sont jamais deplaces, figes ni ecrases.
    private static bool IsBoss(GameObject go)
    {
        return go.GetComponentInParent<BossHealth>() != null
            || go.GetComponentInParent<BossController>() != null
            || go.GetComponentInParent<BossZombieWaypointController>() != null
            || go.GetComponentInParent<BossObstacle>() != null;
    }

    private static void Spawn(GameObject prefab, Vector3 position, float lifetime)
    {
        if (prefab == null) return;
        var instance = Instantiate(prefab, position, Quaternion.identity);
        Destroy(instance, lifetime);
    }

    private static float GetFloat(object target, string field)
    {
        var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return f != null && f.FieldType == typeof(float) ? (float)f.GetValue(target) : 0f;
    }

    private static void SetFloat(object target, string field, float value)
    {
        var f = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (f != null && f.FieldType == typeof(float)) f.SetValue(target, value);
    }
}
