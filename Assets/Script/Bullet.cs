using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject hitVFX; // VFX à l'IMPACT (ex: explosion, étincelles)
    [SerializeField] private int damage = 10;   // Dégâts infligés

    private void OnTriggerEnter(Collider other)
    {
        // Applique des dégâts si l'objet a un composant Health
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
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