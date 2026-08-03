using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossHealth : MonoBehaviour
{
    // Événements pour mettre à jour l'UI (Barre de vie) et gérer la mort
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [Header("Paramètres du Boss")]
    [SerializeField] private int maxHealth = 500; // Plus de vie qu'un ennemi classique
    [SerializeField] private int pointsValue = 500; // Plus de points de score

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;

    private int _currentHealth;

    private void Start()
    {
        Initialize(maxHealth);
    }

    /// <summary>
    /// Permet d'initialiser ou réinitialiser la vie du boss
    /// </summary>
    public void Initialize(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;

        // Notifie l'UI (la barre de vie) du changement de PV
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();

        // Ajout du score
        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddZombieScore(pointsValue);
        }

        // Effet visuel de mort
        if (deathVFX != null)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }

        // Relance l'avancée du véhicule du joueur maintenant que le boss est mort
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var carController = playerObj.GetComponent<CarLaneController>();
            if (carController != null)
            {
                carController.isStoppedForBoss = false;
            }
        }

        Destroy(gameObject);
    }
}