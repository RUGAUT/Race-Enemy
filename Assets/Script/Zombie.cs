using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField] private int pointsValue = 1; // Points à ajouter au score des zombies
    [SerializeField] private float destroyDelay = 0.5f; // Délai avant de détruire le zombie après la collision
    [SerializeField] private GameObject vfxPrefab; // Prefab pour l'effet visuel de disparition

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Vérifier si le véhicule entre en collision avec le zombie
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>(); // Trouver le ScoreManager dans la scène

            if (scoreManager != null)
            {
                scoreManager.AddZombieScore(pointsValue); // Ajouter des points au score des zombies
            }

            // Désactiver le zombie avec un délai
            gameObject.SetActive(false); // Désactiver le zombie
            InstantiateVFX(); // Appeler la méthode pour instancier le VFX
            Invoke(nameof(DestroyZombie), destroyDelay); // Appel de la méthode pour détruire le zombie après un délai
        }
    }

    private void InstantiateVFX()
    {
        if (vfxPrefab != null)
        {
            // Instancier le prefab VFX à la position du zombie
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Détruire le VFX après 2 secondes (ajustez selon vos besoins)
        }
    }

    private void DestroyZombie()
    {
        Destroy(gameObject); // Détruire le zombie
    }
}
