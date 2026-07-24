using System.Collections;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100; // Santé maximale du véhicule
    private int currentHealth;

    [SerializeField] private GameObject damageVFX; // VFX pour la prise de dégâts
    [SerializeField] private GameObject destructionVFX; // VFX pour la destruction du véhicule
    [SerializeField] private Transform vfxSpawnPoint; // Point d'apparition des VFX
    [SerializeField] private Transform barrierSpawnPoint; // Point d'apparition pour la barrière
    [SerializeField] private float invincibilityDuration = 5f; // Durée par défaut d'invincibilité après avoir pris des dégâts
    private bool isInvincible = false; // Vérifie si le véhicule est temporairement invincible
    private float invincibilityEndTime; // Temps de fin de l'invincibilité
    private GameObject activeBarrier; // Référence à la barrière active

    [SerializeField] private GameObject barrierPrefab; // Prefab de la barrière

    public int MaxHealth => maxHealth; // Propriété pour accéder à la santé maximale
    public int CurrentHealth => currentHealth; // Propriété pour accéder à la santé actuelle

    private void Start()
    {
        currentHealth = maxHealth; // Initialiser la santé au maximum
    }

    public void TakeDamage(int damageAmount)
    {
        if (!isInvincible) // Vérifier si le véhicule est invincible
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
            else
            {
                StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
            }
        }
        else
        {
            Debug.Log("Véhicule est invincible et ne prend pas de dégâts.");
        }
    }

    public void RestoreHealth(int restoreAmount)
    {
        currentHealth += restoreAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Véhicule a récupéré {restoreAmount} de santé. Santé actuelle : {currentHealth}");
    }

    public void ActivateInvulnerability(float duration)
    {
        // Activer l'invulnérabilité en démarrant une nouvelle coroutine
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    public void ExtendInvulnerability(float additionalDuration)
    {
        // Si déjà invincible, prolonger l'invulnérabilité
        invincibilityEndTime += additionalDuration;
        Debug.Log("Invulnérabilité prolongée.");
    }

    public bool IsInvulnerable()
    {
        // Vérifie si l'invulnérabilité est toujours active
        return Time.time < invincibilityEndTime;
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        invincibilityEndTime = Time.time + duration; // Calculer le temps de fin de l'invulnérabilité
        Debug.Log("Véhicule est maintenant invulnérable.");

        yield return new WaitForSeconds(duration);

        if (Time.time >= invincibilityEndTime)
        {
            isInvincible = false; // Désactiver l'invulnérabilité après la durée
            Debug.Log("Véhicule est à nouveau vulnérable.");
        }
    }

    public void ActivateBarrier(float duration)
    {
        if (activeBarrier != null)
        {
            Destroy(activeBarrier);
        }

        if (barrierPrefab != null && barrierSpawnPoint != null)
        {
            activeBarrier = Instantiate(barrierPrefab, barrierSpawnPoint.position, barrierSpawnPoint.rotation);
            activeBarrier.transform.SetParent(transform);

            StartCoroutine(DisableBarrier(duration));
        }
        else
        {
            Debug.LogError("Le prefab de la barrière ou le point d'apparition n'est pas assigné !");
        }
    }

    private IEnumerator DisableBarrier(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (activeBarrier != null)
        {
            Destroy(activeBarrier);
            activeBarrier = null;
            Debug.Log("Barrière désactivée !");
        }
    }

    private void DestroyVehicle()
    {
        if (destructionVFX != null)
        {
            Instantiate(destructionVFX, vfxSpawnPoint.position, Quaternion.identity);
        }

        Debug.Log("Véhicule détruit!");
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
