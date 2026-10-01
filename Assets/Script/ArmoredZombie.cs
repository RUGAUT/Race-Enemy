using System.Collections;
using UnityEngine;

// Zombie blindé :
// - Tant que son armure tient, les balles ne lui font AUCUN dégât :
//   elles ne font qu'user lentement l'armure.
// - Une explosion (grenade, kamikaze...) brise l'armure d'un coup
//   et blesse le zombie normalement.
// - Une fois l'armure brisée, il redevient un zombie normal que les balles peuvent tuer.
//
// À placer EN PLUS de tes scripts Zombie et Health (qui gèrent déjà
// le déplacement, le contact avec le véhicule et la mort).
[RequireComponent(typeof(Health))]
public class ArmoredZombie : MonoBehaviour
{
    [Header("=== Armure ===")]
    [Tooltip("L'objet 3D de l'armure (casque, bouclier, plaques...), enfant du zombie")]
    [SerializeField] private GameObject armorVisual;
    [Tooltip("Résistance de l'armure face aux balles")]
    [SerializeField] private int armorHealth = 60;
    [Tooltip("Part des dégâts d'une balle qui abîme l'armure (0 = balles totalement inutiles, 1 = dégâts complets)")]
    [SerializeField, Range(0f, 1f)] private float bulletEfficiency = 0.25f;
    [Tooltip("Si coché, une explosion brise l'armure instantanément")]
    [SerializeField] private bool explosionsBreakArmor = true;

    [Header("=== Effets ===")]
    [Tooltip("Étincelles quand une balle rebondit sur l'armure")]
    [SerializeField] private GameObject bulletBlockedVFX;
    [SerializeField] private GameObject armorBreakVFX;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.05f;

    [Header("=== Éjection de l'armure ===")]
    [SerializeField] private float armorEjectForce = 6f;
    [SerializeField] private float armorDebrisLifetime = 3f;

    private Health health;
    private float currentArmor;
    private Renderer[] armorRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    public bool IsArmorActive => currentArmor > 0f;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnHealthChanged += OnHealthChanged;

        currentArmor = armorHealth;
        propertyBlock = new MaterialPropertyBlock();

        if (armorVisual != null)
        {
            armorRenderers = armorVisual.GetComponentsInChildren<Renderer>();
        }
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= OnHealthChanged;
    }

    // --- Appelée par le script Bullet quand une balle touche ce zombie ---
    public void AbsorbBulletHit(int bulletDamage, Vector3 hitPoint)
    {
        if (!IsArmorActive) return;

        if (bulletBlockedVFX != null)
        {
            Instantiate(bulletBlockedVFX, hitPoint, Quaternion.identity);
        }

        FlashArmor();

        currentArmor -= bulletDamage * bulletEfficiency;
        if (currentArmor <= 0f)
        {
            BreakArmor();
        }
    }

    // --- Appelée par Health à chaque changement de vie ---
    // Tant que l'armure est active, les balles sont bloquées.
    // Si la vie baisse quand même, c'est donc forcément une explosion.
    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (!IsArmorActive || !explosionsBreakArmor) return;

        // Ignore l'initialisation de la vie au démarrage
        if (currentHealth >= maxHealth) return;

        BreakArmor();
    }

    private void BreakArmor()
    {
        if (currentArmor <= 0f && armorVisual == null) return;

        currentArmor = 0f;

        if (armorBreakVFX != null)
        {
            Instantiate(armorBreakVFX, transform.position + Vector3.up, Quaternion.identity);
        }

        if (armorVisual != null)
        {
            EjectArmor(armorVisual);
            armorVisual = null;
        }
    }

    // L'armure se détache et vole en l'air
    private void EjectArmor(GameObject armor)
    {
        armor.transform.SetParent(null, true);

        // Désactive ses colliders pour qu'elle ne gêne pas le véhicule
        foreach (Collider col in armor.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        Rigidbody rb = armor.GetComponent<Rigidbody>();
        if (rb == null) rb = armor.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 ejectDirection = (Vector3.up * 1.5f + Vector3.forward + Random.insideUnitSphere * 0.5f).normalized;
        rb.AddForce(ejectDirection * armorEjectForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * armorEjectForce, ForceMode.Impulse);

        Destroy(armor, armorDebrisLifetime);
    }

    private void FlashArmor()
    {
        if (armorRenderers == null || armorRenderers.Length == 0) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetArmorTint(true);
        yield return new WaitForSeconds(hitFlashDuration);
        SetArmorTint(false);
        flashCoroutine = null;
    }

    private void SetArmorTint(bool active)
    {
        if (armorRenderers == null) return;

        foreach (Renderer rend in armorRenderers)
        {
            if (rend == null) continue;

            if (active)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", hitFlashColor); // URP
                propertyBlock.SetColor("_Color", hitFlashColor);     // Built-in
                rend.SetPropertyBlock(propertyBlock);
            }
            else
            {
                rend.SetPropertyBlock(null);
            }
        }
    }
}