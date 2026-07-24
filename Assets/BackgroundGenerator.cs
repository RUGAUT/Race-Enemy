using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> backgroundPrefabs; // Liste des prefabs d'arrière-plan
    [SerializeField] private int initialBackgroundCount = 5; // Nombre initial de morceaux d'arrière-plan générés
    [SerializeField] private Transform player; // Référence au joueur (ou au véhicule)
    [SerializeField] private float backgroundLength = 30f; // Longueur de chaque morceau d'arrière-plan
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 0f); // Position initiale de la zone de spawn
    [SerializeField] private Vector3 spawnSize = new Vector3(10f, 0f, 30f); // Taille de la zone de spawn (X : largeur, Z : longueur)
    [SerializeField] private Vector3 backgroundRotation = Vector3.zero; // Rotation de l'apparition de l'arrière-plan
    [SerializeField] private float backgroundLifetime = 5f; // Durée de vie d'un morceau d'arrière-plan avant de disparaître
    [SerializeField] private float minDistanceBetweenBackgrounds = 1f; // Distance minimale entre deux morceaux d'arrière-plan

    private float nextSpawnZ; // Position Z pour le prochain morceau d'arrière-plan
    private Queue<GameObject> activeBackgrounds = new Queue<GameObject>(); // Files d'attente des arrière-plans actifs

    void Start()
    {
        nextSpawnZ = spawnPosition.z; // Initialisation à la position de spawn définie

        // Générer les morceaux d'arrière-plan initiaux
        for (int i = 0; i < initialBackgroundCount; i++)
        {
            SpawnBackground();
        }
    }

    void Update()
    {
        // Si le joueur dépasse un certain point, on génère un nouveau morceau d'arrière-plan
        if (player.position.z > nextSpawnZ - (initialBackgroundCount * backgroundLength))
        {
            SpawnBackground();
        }
    }

    // Générer un nouveau morceau d'arrière-plan à la position désirée
    private void SpawnBackground()
    {
        // Générer aléatoirement une position de spawn dans la zone spécifiée
        float randomX = Random.Range(-spawnSize.x / 2f, spawnSize.x / 2f); // Largeur aléatoire
        float randomZ = nextSpawnZ; // On garde l'ordre sur l'axe Z pour aligner les arrière-plans

        Vector3 position = new Vector3(randomX + spawnPosition.x, spawnPosition.y, randomZ); // Position finale

        // Vérifier que la position est suffisament éloignée des autres morceaux d'arrière-plan
        if (IsPositionValid(position))
        {
            // Choisir un prefab d'arrière-plan aléatoire
            GameObject backgroundPrefab = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Count)];
            GameObject background = Instantiate(backgroundPrefab, position, Quaternion.Euler(backgroundRotation)); // Instancie le morceau d'arrière-plan avec rotation
            activeBackgrounds.Enqueue(background); // Ajoute le morceau d'arrière-plan à la file d'attente

            nextSpawnZ += backgroundLength; // Met à jour la position Z pour le prochain morceau

            // Démarrer une coroutine pour détruire l'arrière-plan après un certain temps
            StartCoroutine(DestroyBackgroundAfterTime(background, backgroundLifetime));
        }
        else
        {
            // Si la position n'est pas valide, on ne crée pas d'arrière-plan et on essaie de nouveau
            SpawnBackground();
        }
    }

    // Vérifie si la position est valide pour éviter les chevauchements
    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject background in activeBackgrounds)
        {
            if (Vector3.Distance(background.transform.position, position) < minDistanceBetweenBackgrounds)
            {
                return false; // La position est trop proche d'un autre morceau d'arrière-plan
            }
        }
        return true; // La position est valide
    }

    // Coroutine pour détruire l'arrière-plan après une durée spécifiée
    private IEnumerator DestroyBackgroundAfterTime(GameObject background, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        activeBackgrounds.Dequeue(); // Retire l'arrière-plan de la file d'attente avant de le détruire
        Destroy(background); // Détruit le morceau d'arrière-plan
    }

    // Dessiner un gizmo pour voir la zone de spawn (utile pour débogage)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(spawnPosition.x, spawnPosition.y, nextSpawnZ + backgroundLength / 2), new Vector3(spawnSize.x, 0.1f, spawnSize.z));
    }
}
