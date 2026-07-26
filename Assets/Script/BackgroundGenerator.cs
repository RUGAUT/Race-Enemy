using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> backgroundPrefabs;
    [SerializeField] private int initialBackgroundCount = 5;
    // J'ai retiré le Transform player fixe d'ici

    [SerializeField] private float backgroundLength = 30f;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 spawnSize = new Vector3(10f, 0f, 30f);
    [SerializeField] private Vector3 backgroundRotation = Vector3.zero;
    [SerializeField] private float backgroundLifetime = 5f;
    [SerializeField] private float minDistanceBetweenBackgrounds = 1f;

    private float nextSpawnZ;
    private Queue<GameObject> activeBackgrounds = new Queue<GameObject>();

    private Transform activePlayer; // Référence dynamique
    private bool hasInitialized = false; // Sécurité pour le lancement initial

    void Update()
    {
        // 1. Cherche le joueur actuel s'il est manquant ou désactivé
        if (activePlayer == null || !activePlayer.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
        }

        // 2. Met en pause si aucun véhicule n'est sur la scène
        if (activePlayer == null) return;

        // 3. Premier lancement : ne s'exécute qu'une seule fois quand le premier véhicule est trouvé
        if (!hasInitialized)
        {
            nextSpawnZ = spawnPosition.z;
            for (int i = 0; i < initialBackgroundCount; i++)
            {
                SpawnBackground();
            }
            hasInitialized = true;
        }

        // 4. Suit la progression du véhicule ACTIF
        if (activePlayer.position.z > nextSpawnZ - (initialBackgroundCount * backgroundLength))
        {
            SpawnBackground();
        }
    }

    // Nouvelle fonction de recherche automatique
    private void FindActivePlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            activePlayer = playerObj.transform;
        }
    }

    private void SpawnBackground()
    {
        float randomX = Random.Range(-spawnSize.x / 2f, spawnSize.x / 2f);
        float randomZ = nextSpawnZ;

        Vector3 position = new Vector3(randomX + spawnPosition.x, spawnPosition.y, randomZ);

        if (IsPositionValid(position))
        {
            GameObject backgroundPrefab = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Count)];
            GameObject background = Instantiate(backgroundPrefab, position, Quaternion.Euler(backgroundRotation));
            activeBackgrounds.Enqueue(background);

            nextSpawnZ += backgroundLength;

            StartCoroutine(DestroyBackgroundAfterTime(background, backgroundLifetime));
        }
        else
        {
            SpawnBackground();
        }
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject background in activeBackgrounds)
        {
            if (Vector3.Distance(background.transform.position, position) < minDistanceBetweenBackgrounds)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator DestroyBackgroundAfterTime(GameObject background, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        activeBackgrounds.Dequeue();
        Destroy(background);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(spawnPosition.x, spawnPosition.y, nextSpawnZ + backgroundLength / 2), new Vector3(spawnSize.x, 0.1f, spawnSize.z));
    }
}