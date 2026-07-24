using System.Collections.Generic;
using UnityEngine;

public class CustomRoadGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roadPrefab; // Prefab de la route
    [SerializeField] private int initialRoadCount = 5; // Nombre initial de morceaux
    [SerializeField] private Transform player; // Référence au joueur
    [SerializeField] private float roadLength = 30f; // Longueur d'un morceau de route
    [SerializeField] private float spawnDistanceAhead = 50f; // Distance devant le joueur pour spawner
    [SerializeField] private float despawnDistanceBehind = 20f; // Distance derrière le joueur pour supprimer
    [SerializeField] private Vector3 spawnPositionOffset = Vector3.zero; // Décalage de la position de spawn
    [SerializeField] private Vector3 roadRotation = Vector3.zero; // Rotation de la route

    private Queue<GameObject> activeRoads = new Queue<GameObject>();
    private float lastRoadZ; // Position Z du dernier morceau de route généré

    void Start()
    {
        lastRoadZ = player.position.z;
        // Générer les morceaux initiaux devant le joueur
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        // Si le joueur s'approche du dernier morceau de route, en générer un nouveau
        if (player.position.z > lastRoadZ - spawnDistanceAhead)
        {
            SpawnRoad();
        }

        // Supprimer les morceaux de route trop derrière le joueur
        if (activeRoads.Count > 0 && activeRoads.Peek().transform.position.z < player.position.z - despawnDistanceBehind)
        {
            RemoveOldRoad();
        }
    }

    private void SpawnRoad()
    {
        // Position devant le joueur + décalage
        Vector3 spawnPosition = new Vector3(
            spawnPositionOffset.x,
            spawnPositionOffset.y,
            lastRoadZ + roadLength
        );

        GameObject road = Instantiate(
            roadPrefab,
            spawnPosition,
            Quaternion.Euler(roadRotation)
        );

        activeRoads.Enqueue(road);
        lastRoadZ = spawnPosition.z; // Met à jour la position du dernier morceau
    }

    private void RemoveOldRoad()
    {
        if (activeRoads.Count > 0)
        {
            GameObject oldRoad = activeRoads.Dequeue();
            Destroy(oldRoad);
        }
    }

    // Gizmo pour visualiser la zone de spawn (optionnel)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 spawnPos = new Vector3(
            spawnPositionOffset.x,
            spawnPositionOffset.y,
            lastRoadZ
        );
        Gizmos.DrawWireCube(spawnPos, new Vector3(10f, 0.1f, roadLength));
    }
}