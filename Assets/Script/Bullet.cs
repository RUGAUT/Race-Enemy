using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject hitVFX; // VFX à l'IMPACT (ex: explosion, étincelles)
    [SerializeField] private int damage = 10;   // Dégâts infligés

    // --- NOUVEAU : empêche une balle de toucher deux cibles dans la même frame ---
    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // --- NOUVEAU : si la cible porte une armure active, la balle rebondit dessus ---
        ArmoredZombie armoredZombie = other.GetComponentInParent<ArmoredZombie>();
        if (armoredZombie != null && armoredZombie.IsArmorActive)
        {
            hasHit = true;
            armoredZombie.AbsorbBulletHit(damage, transform.position);
            Destroy(gameObject);
            return;
        }

        // --- Ton code d'origine ---
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
        else
        {
            BossHealth bossHealth = other.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                bossHealth.TakeDamage(damage);
            }
        }

        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }

        hasHit = true;
        Destroy(gameObject);
    }
}