using UnityEngine;
using System.Collections;
using UnityEngine.UI; // --- NOUVEAU : Requis pour interagir avec l'UI ---

public class VehicleAttack : MonoBehaviour
{
    [Header("=== Points de Tir (Balles) ===")]
    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private Transform rightFirePoint;

    [Header("=== Points de Tir (VFX Muzzle Flash) ===")]
    [SerializeField] private Transform leftVFXPoint;
    [SerializeField] private Transform rightVFXPoint;

    [Header("=== Préfabs ===")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlashVFX;

    [Header("=== Paramètres de Tir ===")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float vfxLifetime = 0.1f;

    [Header("=== Direction et Rotation ===")]
    [SerializeField] private Vector3 bulletDirection = Vector3.forward;
    [SerializeField] private Vector3 bulletRotationOffset = Vector3.zero;

    [Header("=== Tir Automatique ===")]
    [SerializeField] private bool isAutoFire = true;
    [SerializeField] private float autoFireRate = 0.5f;

    [Header("=== Taille du VFX ===")]
    [SerializeField] private Vector3 muzzleFlashScale = Vector3.one;

    [Header("=== Grenade ===")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform grenadeFirePoint;
    [SerializeField] private float grenadeForwardForce = 50f;
    [SerializeField] private float grenadeUpwardForce = 10f;
    [SerializeField] private float grenadeCooldown = 3f;

    // --- NOUVEAU : Paramètres UI pour le bouton ---
    [Header("=== UI Grenade ===")]
    [SerializeField] private Image grenadeButtonImage;
    [Tooltip("Opacité du bouton pendant le cooldown (0 = invisible, 1 = opaque)")]
    [SerializeField] private float cooldownAlpha = 0.4f;

    private float nextFireTime = 0f;
    private float nextGrenadeTime = 0f;
    private Coroutine autoFireCoroutine;

    private void Start()
    {
        if (leftFirePoint == null) Debug.LogError("LeftFirePoint n'est pas assigné !", this);
        if (rightFirePoint == null) Debug.LogError("RightFirePoint n'est pas assigné !", this);
        if (bulletPrefab == null) Debug.LogError("BulletPrefab n'est pas assigné !", this);
    }

    private void OnEnable()
    {
        if (isAutoFire) autoFireCoroutine = StartCoroutine(AutoFire());
    }

    private void OnDisable()
    {
        if (autoFireCoroutine != null) StopCoroutine(autoFireCoroutine);
    }

    private IEnumerator AutoFire()
    {
        while (true)
        {
            Fire();
            yield return new WaitForSeconds(autoFireRate);
        }
    }

    public void Fire()
    {
        if (!gameObject.activeInHierarchy || Time.time < nextFireTime) return;

        Quaternion bulletRotation = Quaternion.Euler(bulletRotationOffset);
        bool hasFired = false;

        if (leftFirePoint != null && bulletPrefab != null && leftFirePoint.gameObject.activeInHierarchy)
        {
            ShootBullet(leftFirePoint, leftVFXPoint, bulletRotation);
            hasFired = true;
        }

        if (rightFirePoint != null && bulletPrefab != null && rightFirePoint.gameObject.activeInHierarchy)
        {
            ShootBullet(rightFirePoint, rightVFXPoint, bulletRotation);
            hasFired = true;
        }

        if (!hasFired) Debug.LogWarning("Aucun tir effectué !");

        nextFireTime = Time.time + fireRate;
    }

    private void ShootBullet(Transform firePoint, Transform vfxPoint, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = firePoint.TransformDirection(bulletDirection) * bulletSpeed;

        if (muzzleFlashVFX != null)
        {
            Transform vfxTarget = (vfxPoint != null) ? vfxPoint : firePoint;
            GameObject vfx = Instantiate(muzzleFlashVFX, vfxTarget.position, vfxTarget.rotation, vfxTarget);
            vfx.transform.localScale = muzzleFlashScale;
            Destroy(vfx, vfxLifetime);
        }
    }

    public void FireGrenade()
    {
        if (!gameObject.activeInHierarchy || Time.time < nextGrenadeTime) return;

        if (grenadePrefab != null && grenadeFirePoint != null)
        {
            GameObject grenade = Instantiate(grenadePrefab, grenadeFirePoint.position, grenadeFirePoint.rotation);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 force = (transform.forward * grenadeForwardForce) + (Vector3.up * grenadeUpwardForce);
                rb.AddForce(force, ForceMode.Impulse);
            }

            nextGrenadeTime = Time.time + grenadeCooldown;

            // --- NOUVEAU : Déclenche l'effet visuel sur le bouton ---
            if (grenadeButtonImage != null)
            {
                StartCoroutine(GrenadeCooldownVisualRoutine());
            }
        }
    }

    // --- NOUVEAU : Coroutine qui gère l'opacité du bouton ---
    private IEnumerator GrenadeCooldownVisualRoutine()
    {
        // On récupère la couleur actuelle du bouton et on modifie son Alpha
        Color buttonColor = grenadeButtonImage.color;
        buttonColor.a = cooldownAlpha;
        grenadeButtonImage.color = buttonColor;

        // On attend la durée du cooldown
        yield return new WaitForSeconds(grenadeCooldown);

        // On remet l'opacité à fond (1f) une fois le cooldown terminé
        buttonColor.a = 1f;
        grenadeButtonImage.color = buttonColor;
    }
}