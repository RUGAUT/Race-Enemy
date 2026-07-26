using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    // NOUVEAU : Le nombre de points que rapporte cet ennemi quand il meurt
    [SerializeField] private int pointsValue = 10;

    private int currentHealth;

    [Header("VFX")]
    [SerializeField] private GameObject deathVFX;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // NOUVEAU : On ajoute les points au score avant de détruire l'objet
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddZombieScore(pointsValue);
        }

        if (deathVFX != null)
        {
            Instantiate(deathVFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}