using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roadPrefab; // Le prefab de la route
    [SerializeField] private int initialRoadCount = 5; // Nombre initial de morceaux de route générés
    [SerializeField] private Transform player; // Référence au joueur (ou au véhicule)
    [SerializeField] private float roadLength = 30f; // Longueur de chaque morceau de route
    [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 0f); // Position initiale de la zone de spawn
    [SerializeField] private Vector3 spawnSize = new Vector3(10f, 0f, 30f); // Taille de la zone de spawn (X : largeur, Z : longueur)

    private float nextSpawnZ; // Position Z pour le prochain morceau de route
    private Queue<GameObject> activeRoads = new Queue<GameObject>(); // Files d'attente des routes actives

    void Start()
    {
        nextSpawnZ = spawnPosition.z; // Initialisation à la position de spawn définie

        // Générer les morceaux de route initiaux
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        // Si le joueur dépasse un certain point, on génère un nouveau morceau de route
        if (player.position.z > nextSpawnZ - (initialRoadCount * roadLength))
        {
            SpawnRoad();
            RemoveOldRoad();
        }
    }

    // Générer un nouveau morceau de route à la position désirée
    private void SpawnRoad()
    {
        // Générer aléatoirement une position de spawn dans la zone spécifiée
        float randomX = Random.Range(-spawnSize.x / 2f, spawnSize.x / 2f); // Largeur aléatoire
        float randomZ = nextSpawnZ; // On garde l'ordre sur l'axe Z pour aligner les routes

        Vector3 position = new Vector3(randomX + spawnPosition.x, spawnPosition.y, randomZ); // Position finale
        GameObject road = Instantiate(roadPrefab, position, Quaternion.identity); // Instancie le morceau de route
        activeRoads.Enqueue(road); // Ajoute le morceau de route à la file d'attente

        nextSpawnZ += roadLength; // Met à jour la position Z pour le prochain morceau
    }

    // Supprimer l'ancien morceau de route pour éviter d'encombrer la scène
    private void RemoveOldRoad()
    {
        GameObject oldRoad = activeRoads.Dequeue();
        Destroy(oldRoad); // Détruit l'ancien morceau de route
    }

    // Dessiner un gizmo pour voir la zone de spawn (utile pour débogage)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(spawnPosition.x, spawnPosition.y, nextSpawnZ + roadLength / 2), new Vector3(spawnSize.x, 0.1f, spawnSize.z));
    }
}
