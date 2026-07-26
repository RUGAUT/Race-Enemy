using UnityEngine;
using System.Collections;

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

    private float nextFireTime = 0f;
    private Coroutine autoFireCoroutine;

    private void Start()
    {
        if (leftFirePoint == null) Debug.LogError("LeftFirePoint n'est pas assigné !", this);
        if (rightFirePoint == null) Debug.LogError("RightFirePoint n'est pas assigné !", this);
        if (bulletPrefab == null) Debug.LogError("BulletPrefab n'est pas assigné !", this);

        if (leftVFXPoint == null || rightVFXPoint == null)
            Debug.LogWarning("Les points VFX ne sont pas assignés. Le flash apparaîtra à l'endroit de la balle.", this);
    }

    // OnEnable se lance à chaque fois que le véhicule est activé (pratique pour le changement de véhicule)
    private void OnEnable()
    {
        if (isAutoFire)
        {
            autoFireCoroutine = StartCoroutine(AutoFire());
        }
    }

    // OnDisable s'assure d'arrêter le tir si le véhicule est caché
    private void OnDisable()
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
        // NOUVEAU : Si ce véhicule est désactivé, on annule la fonction instantanément !
        // Ça empêche le bouton d'agir sur les véhicules cachés et évite les erreurs.
        if (!gameObject.activeInHierarchy) return;

        if (Time.time < nextFireTime) return;

        Quaternion bulletRotation = Quaternion.Euler(bulletRotationOffset);
        bool hasFired = false;

        // --- TIR À GAUCHE ---
        if (leftFirePoint != null && bulletPrefab != null && leftFirePoint.gameObject.activeInHierarchy)
        {
            GameObject bullet = Instantiate(bulletPrefab, leftFirePoint.position, leftFirePoint.rotation * bulletRotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = leftFirePoint.TransformDirection(bulletDirection) * bulletSpeed;
            }

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
            GameObject bullet = Instantiate(bulletPrefab, rightFirePoint.position, rightFirePoint.rotation * bulletRotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = rightFirePoint.TransformDirection(bulletDirection) * bulletSpeed;
            }

            if (muzzleFlashVFX != null)
            {
                Transform vfxTransform = (rightVFXPoint != null) ? rightVFXPoint : rightFirePoint;
                GameObject vfx = Instantiate(muzzleFlashVFX, vfxTransform.position, vfxTransform.rotation, vfxTransform);
                vfx.transform.localScale = muzzleFlashScale;
                Destroy(vfx, vfxLifetime);
            }
            hasFired = true;
        }

        // Optionnel : J'ai passé ça en LogWarning plutôt qu'en LogError pour éviter le blocage d'Unity "Error Pause"
        if (!hasFired)
        {
            Debug.LogWarning("Aucun tir effectué ! Vérifie les FirePoint et bulletPrefab.");
        }

        nextFireTime = Time.time + fireRate;
    }
}