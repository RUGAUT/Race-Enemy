using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    // Événements permettant de découpler l'UI de la logique de santé (Zéro allocation dans l'Update)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int pointsValue = 10;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;

    private int _currentHealth;

    private void Start()
    {
        Initialize(maxHealth);
    }

    /// <summary>
    /// Permet d'écraser la vie maximale dynamiquement
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
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();

        // FIX : Appel direct de la méthode (héritée de MonoBehaviour/Behaviour/Component/Object) 
        // ou utilisation du namespace explicite UnityEngine.Object pour éviter le conflit avec System.Object.
        var scoreManager = FindFirstObjectByType<ScoreManager>();

        if (scoreManager != null)
        {
            scoreManager.AddZombieScore(pointsValue);
        }

        if (deathVFX != null)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}