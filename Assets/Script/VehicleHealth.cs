using System.Collections;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("VFX & Spawns")]
    [SerializeField] private GameObject damageVFX;
    [SerializeField] private GameObject destructionVFX;
    [SerializeField] private Transform vfxSpawnPoint;
    [SerializeField] private Transform barrierSpawnPoint;

    [Header("Barrier Settings")]
    [SerializeField] private GameObject barrierPrefab;
    [SerializeField] private GameObject barrierDestroyObstacleVFX;

    private bool isInvincible = false;
    private float invincibilityEndTime;
    private GameObject activeBarrier;

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
        if (activeBarrier != null)
        {
            return false;
        }

        if (barrierPrefab != null && barrierSpawnPoint != null)
        {
            activeBarrier = Instantiate(barrierPrefab, barrierSpawnPoint.position, barrierSpawnPoint.rotation);
            activeBarrier.transform.SetParent(transform);

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
            isInvincible = false;
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

    // NOUVEAU : Fonction appelée par l'obstacle quand il est détruit par la barrière
    public void PlayBarrierDestroyVFX(Vector3 position)
    {
        if (barrierDestroyObstacleVFX != null)
        {
            Instantiate(barrierDestroyObstacleVFX, position, Quaternion.identity);
        }
    }
}