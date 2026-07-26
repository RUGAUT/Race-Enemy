using System.Collections;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [SerializeField] private GameObject damageVFX;
    [SerializeField] private GameObject destructionVFX;
    [SerializeField] private Transform vfxSpawnPoint;
    [SerializeField] private Transform barrierSpawnPoint;

    // J'ai retiré la variable invincibilityDuration d'ici car c'est le Bonus qui décide de la durée maintenant.
    private bool isInvincible = false;
    private float invincibilityEndTime;
    private GameObject activeBarrier;

    [SerializeField] private GameObject barrierPrefab;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public bool HasActiveBarrier => activeBarrier != null;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isInvincible)
        {
            currentHealth -= damageAmount;
            Debug.Log($"Véhicule a pris {damageAmount} dégâts. Santé restante : {currentHealth}");

            if (damageVFX != null)
            {
                Instantiate(damageVFX, vfxSpawnPoint.position, Quaternion.identity);
            }

            if (currentHealth <= 0)
            {
                DestroyVehicle();
            }
            // SUPPRESSION ICI : Le véhicule ne devient plus invincible automatiquement après un dégât !
        }
        else
        {
            Debug.Log("Véhicule est invincible (Barrière active) et ne prend pas de dégâts.");
        }
    }

    public void RestoreHealth(int restoreAmount)
    {
        currentHealth += restoreAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void ActivateInvulnerability(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    public bool IsInvulnerable()
    {
        return Time.time < invincibilityEndTime;
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        invincibilityEndTime = Time.time + duration;

        yield return new WaitForSeconds(duration);

        if (Time.time >= invincibilityEndTime)
        {
            isInvincible = false;
            Debug.Log("Fin de la barrière, véhicule à nouveau vulnérable.");
        }
    }

    public bool ActivateBarrier(float duration)
    {
        // Si une barrière est déjà active, on bloque
        if (activeBarrier != null)
        {
            return false;
        }

        // Sinon, on la crée
        if (barrierPrefab != null && barrierSpawnPoint != null)
        {
            activeBarrier = Instantiate(barrierPrefab, barrierSpawnPoint.position, barrierSpawnPoint.rotation);
            activeBarrier.transform.SetParent(transform);

            // On active l'invincibilité ici
            ActivateInvulnerability(duration);

            StartCoroutine(DisableBarrierAfterTime(duration));
            return true;
        }

        return false;
    }

    private IEnumerator DisableBarrierAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (activeBarrier != null)
        {
            Destroy(activeBarrier);
            activeBarrier = null;
            isInvincible = false; // Sécurité supplémentaire
        }
    }

    private void DestroyVehicle()
    {
        if (destructionVFX != null)
        {
            Instantiate(destructionVFX, vfxSpawnPoint.position, Quaternion.identity);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            ObstacleDamage obstacleDamage = other.GetComponent<ObstacleDamage>();
            if (obstacleDamage != null)
            {
                TakeDamage(obstacleDamage.GetDamageAmount());
            }
        }
    }
}