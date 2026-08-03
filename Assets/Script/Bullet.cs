using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject hitVFX; // VFX à l'IMPACT (ex: explosion, étincelles)
    [SerializeField] private int damage = 10;   // Dégâts infligés

    private void OnTriggerEnter(Collider other)
    {
        // 1. Vérifie si c'est un ennemi standard
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else
        {
            // 2. Sinon, vérifie si c'est le Boss
            BossHealth bossHealth = other.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damage);
            }
        }

        // Instancie le VFX d'impact (ex: explosion)
        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }

        // Détruit la balle
        Destroy(gameObject);
    }
}