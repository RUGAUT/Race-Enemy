using UnityEngine;
using System.Collections;

public class VehicleAttack : MonoBehaviour
{
    [Header("=== Points de Tir (Balles) ===")]
    [SerializeField] private Transform leftFirePoint;   // Point de tir à GAUCHE (Éloigné pour la balle)
    [SerializeField] private Transform rightFirePoint;  // Point de tir à DROITE (Éloigné pour la balle)

    [Header("=== Points de Tir (VFX Muzzle Flash) ===")]
    [SerializeField] private Transform leftVFXPoint;    // Point pour le flash GAUCHE (Proche du canon)
    [SerializeField] private Transform rightVFXPoint;   // Point pour le flash DROITE (Proche du canon)

    [Header("=== Préfabs ===")]
    [SerializeField] private GameObject bulletPrefab;    // Prefab de la balle
    [SerializeField] private GameObject muzzleFlashVFX;  // VFX de tir (flash)

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

    private float nextFireTime = 0f;
    private Coroutine autoFireCoroutine;

    private void Start()
    {
        // Vérifie que les FirePoint sont assignés
        if (leftFirePoint == null) Debug.LogError("LeftFirePoint n'est pas assigné !", this);
        if (rightFirePoint == null) Debug.LogError("RightFirePoint n'est pas assigné !", this);
        if (bulletPrefab == null) Debug.LogError("BulletPrefab n'est pas assigné !", this);

        // Avertissement si les points VFX ne sont pas assignés
        if (leftVFXPoint == null || rightVFXPoint == null)
            Debug.LogWarning("Les points VFX ne sont pas assignés. Le flash apparaîtra à l'endroit de la balle.", this);

        if (isAutoFire)
        {
            autoFireCoroutine = StartCoroutine(AutoFire());
        }
    }

    private void OnDestroy()
    {
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
        }
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
        if (Time.time < nextFireTime) return;

        Quaternion bulletRotation = Quaternion.Euler(bulletRotationOffset);
        bool hasFired = false;

        // --- TIR À GAUCHE ---
        if (leftFirePoint != null && bulletPrefab != null && leftFirePoint.gameObject.activeInHierarchy)
        {
            // 1. Instancie la balle sur le FirePoint (éloigné)
            GameObject bullet = Instantiate(bulletPrefab, leftFirePoint.position, leftFirePoint.rotation * bulletRotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = leftFirePoint.TransformDirection(bulletDirection) * bulletSpeed;
            }

            // 2. Instancie le VFX sur le VFXPoint (proche), ou sur le FirePoint en secours
            if (muzzleFlashVFX != null)
            {
                Transform vfxTransform = (leftVFXPoint != null) ? leftVFXPoint : leftFirePoint;
                GameObject vfx = Instantiate(muzzleFlashVFX, vfxTransform.position, vfxTransform.rotation, vfxTransform);
                vfx.transform.localScale = muzzleFlashScale;
                Destroy(vfx, vfxLifetime);
            }
            hasFired = true;
        }

        // --- TIR À DROITE ---
        if (rightFirePoint != null && bulletPrefab != null && rightFirePoint.gameObject.activeInHierarchy)
        {
            // 1. Instancie la balle sur le FirePoint (éloigné)
            GameObject bullet = Instantiate(bulletPrefab, rightFirePoint.position, rightFirePoint.rotation * bulletRotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = rightFirePoint.TransformDirection(bulletDirection) * bulletSpeed;
            }

            // 2. Instancie le VFX sur le VFXPoint (proche), ou sur le FirePoint en secours
            if (muzzleFlashVFX != null)
            {
                Transform vfxTransform = (rightVFXPoint != null) ? rightVFXPoint : rightFirePoint;
                GameObject vfx = Instantiate(muzzleFlashVFX, vfxTransform.position, vfxTransform.rotation, vfxTransform);
                vfx.transform.localScale = muzzleFlashScale;
                Destroy(vfx, vfxLifetime);
            }
            hasFired = true;
        }

        if (!hasFired)
        {
            Debug.LogError("Aucun tir effectué ! Vérifie les FirePoint et bulletPrefab.");
        }

        nextFireTime = Time.time + fireRate;
    }
}