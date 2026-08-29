using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    // Un menu pour choisir comment on veut inverser les décors
    public enum InversionMode { None, MirrorScaleX, Rotate180Y }

    [SerializeField] private List<GameObject> backgroundPrefabs;
    [SerializeField] private int initialBackgroundCount = 5;

    [SerializeField] private float backgroundLength = 30f;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 spawnSize = new Vector3(10f, 0f, 30f);
    [SerializeField] private Vector3 backgroundRotation = Vector3.zero;

    // --- NOUVEAU : On remplace le "Time" par la "Distance" ---
    [Tooltip("Distance derrière le véhicule avant de détruire le décor (ex: 40)")]
    [SerializeField] private float destroyDistanceBehindPlayer = 40f;

    [SerializeField] private float minDistanceBetweenBackgrounds = 1f;

    [Header("Anti-Répétition (Variations)")]
    [Tooltip("Choisis comment inverser aléatoirement les décors pour éviter la répétition visuelle.")]
    [SerializeField] private InversionMode inversionMode = InversionMode.MirrorScaleX;

    private float nextSpawnZ;
    private Queue<GameObject> activeBackgrounds = new Queue<GameObject>();

    private Transform activePlayer;
    private bool hasInitialized = false;

    void Update()
    {
        if (activePlayer == null || !activePlayer.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
        }

        if (activePlayer == null) return;

        // 1. Initialisation de départ
        if (!hasInitialized)
        {
            nextSpawnZ = spawnPosition.z;
            for (int i = 0; i < initialBackgroundCount; i++)
            {
                SpawnBackground();
            }
            hasInitialized = true;
        }

        // 2. Apparition de nouveaux décors devant la voiture
        if (activePlayer.position.z > nextSpawnZ - (initialBackgroundCount * backgroundLength))
        {
            SpawnBackground();
        }

        // --- NOUVEAU : 3. Destruction basée sur la POSITION au lieu du temps ---
        if (activeBackgrounds.Count > 0)
        {
            // Regarde le décor le plus ancien (le plus loin derrière)
            GameObject oldestBackground = activeBackgrounds.Peek();

            // Si le décor est détruit pour une autre raison, on nettoie la liste
            if (oldestBackground == null)
            {
                activeBackgrounds.Dequeue();
            }
            // Si le joueur a dépassé le décor de X mètres, on le détruit
            else if (activePlayer.position.z > oldestBackground.transform.position.z + destroyDistanceBehindPlayer)
            {
                activeBackgrounds.Dequeue();
                Destroy(oldestBackground);
            }
        }
    }

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

            // --- GESTION DE L'INVERSION ALÉATOIRE ---
            Vector3 finalRotation = backgroundRotation;
            bool shouldInvert = Random.value > 0.5f; // 50% de chance d'être inversé

            // Si on a choisi de faire faire un demi-tour (Rotation Y)
            if (shouldInvert && inversionMode == InversionMode.Rotate180Y)
            {
                finalRotation.y += 180f;
            }

            GameObject background = Instantiate(backgroundPrefab, position, Quaternion.Euler(finalRotation));

            // Si on a choisi l'effet miroir (Scale X)
            if (shouldInvert && inversionMode == InversionMode.MirrorScaleX)
            {
                Vector3 currentScale = background.transform.localScale;
                currentScale.x *= -1f; // Inverse l'échelle sur l'axe X
                background.transform.localScale = currentScale;
            }
            // ----------------------------------------

            activeBackgrounds.Enqueue(background);
            nextSpawnZ += backgroundLength;
        }
        else
        {
            SpawnBackground(); // Réessaye si la position n'est pas valide
        }
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject background in activeBackgrounds)
        {
            if (background != null && Vector3.Distance(background.transform.position, position) < minDistanceBetweenBackgrounds)
            {
                return false;
            }
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(spawnPosition.x, spawnPosition.y, nextSpawnZ + backgroundLength / 2), new Vector3(spawnSize.x, 0.1f, spawnSize.z));
    }
}