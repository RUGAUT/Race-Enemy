using UnityEngine;

public class GrenadeBehavior : MonoBehaviour
{
    [Header("Paramètres d'Explosion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explosionDamage = 150;
    [SerializeField] private GameObject explosionVFX;

    [Header("Détection pour l'Explosion")]
    [Tooltip("La grenade explose si elle touche un objet avec ce tag (ex: 'Road')")]
    [SerializeField] private string groundTag = "Road";
    [Tooltip("La grenade explose AUSSI si elle touche un objet avec ce tag (ex: 'Enemy' ou 'Zombie')")]
    [SerializeField] private string enemyTag = "Zombie";

    [Header("Cibles des Dégâts (AoE)")]
    [Tooltip("Le tag de tes obstacles pour les détruire dans la zone")]
    [SerializeField] private string obstacleTag = "Obstacle";

    private bool hasExploded = false;

    // 1. Détection des collisions solides (ex: la Route)
    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;

        if (collision.gameObject.CompareTag(groundTag) || collision.gameObject.CompareTag(enemyTag))
        {
            Explode();
        }
    }

    // --- NOUVEAU ---
    // 2. Détection des zones (Is Trigger) (ex: les Zombies)
    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.gameObject.CompareTag(groundTag) || other.gameObject.CompareTag(enemyTag))
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag(obstacleTag))
            {
                Destroy(hit.gameObject);
                continue;
            }

            hit.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}