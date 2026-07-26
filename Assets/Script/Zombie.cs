using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Paramètres de Collision")]
    [SerializeField] private int pointsValue = 1; // Points à ajouter au score
    [SerializeField] private float destroyDelay = 0.5f; // Délai avant destruction
    [SerializeField] private int crashDamage = 20; // Dégâts infligés au véhicule quand on l'écrase

    [Header("VFX")]
    [SerializeField] private GameObject vfxPrefab; // VFX de disparition

    private Health health; // Référence au composant Health

    private void Start()
    {
        // Récupère le composant Health (doit être attaché au même GameObject)
        health = GetComponent<Health>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si le zombie touche le véhicule
        if (other.CompareTag("Player"))
        {
            // 1. Infliger des dégâts au véhicule
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                vehicleHealth.TakeDamage(crashDamage);
            }

            // 2. Ajouter les points au score (puisqu'il a été tué)
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddZombieScore(pointsValue);
            }

            // 3. Lancer la séquence de mort du zombie
            gameObject.SetActive(false);
            InstantiateVFX();
            Invoke(nameof(DestroyZombie), destroyDelay);
        }
    }

    private void InstantiateVFX()
    {
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    private void DestroyZombie()
    {
        Destroy(gameObject);
    }
}