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
    [SerializeField] private float invincibilityDuration = 5f;
    private bool isInvincible = false;
    private float invincibilityEndTime;
    private GameObject activeBarrier;

    [SerializeField] private GameObject barrierPrefab;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public float GetRemainingInvincibilityTime()
    {
        return Mathf.Max(0f, invincibilityEndTime - Time.time);
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
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    public void ExtendInvulnerability(float additionalDuration)
    {
        invincibilityEndTime += additionalDuration;
        Debug.Log("Invulnérabilité prolongée jusqu'à : " + invincibilityEndTime);
    }

    public bool IsInvulnerable()
    {
        return Time.time < invincibilityEndTime;
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        invincibilityEndTime = Time.time + duration;
        Debug.Log("Véhicule est maintenant invulnérable jusqu'à : " + invincibilityEndTime);

        yield return new WaitForSeconds(duration);

        if (Time.time >= invincibilityEndTime)
        {
            isInvincible = false;
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
            StartCoroutine(DisableBarrierAfterTime(duration));
        }
        else
        {
            Debug.LogError("Le prefab de la barrière ou le point d'apparition n'est pas assigné !");
        }
    }

    private IEnumerator DisableBarrierAfterTime(float duration)
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