using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnDistance = 50f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 50f);
    [SerializeField] private Color gizmoColor = Color.blue;

    private Transform vehicle;
    private CarLaneController carController;

    // --- NOUVEAU : positions des points de spawn mémorisées au démarrage ---
    // On n'utilise que X et Y (Z dépend du véhicule), donc on n'a plus besoin
    // des Transforms pendant la partie, même s'ils sont détruits.
    private readonly List<Vector2> lanePositions = new List<Vector2>();

    private void Start()
    {
        // Mémorise la position X/Y de chaque point de spawn valide
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                lanePositions.Add(new Vector2(point.position.x, point.position.y));
            }
        }

        if (lanePositions.Count == 0)
        {
            Debug.LogWarning("[ObstacleSpawner] Aucun Spawn Point assigné !", this);
        }

        RefreshVehicle();
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Retrouve le véhicule s'il a disparu ou a été changé
            if (vehicle == null || !vehicle.gameObject.activeInHierarchy)
            {
                RefreshVehicle();
            }

            // On vérifie si la voiture est arrêtée pour le boss
            if (carController != null && carController.isStoppedForBoss)
            {
                continue;
            }

            SpawnObstacle();
        }
    }

    private void RefreshVehicle()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            vehicle = playerObj.transform;
            carController = playerObj.GetComponent<CarLaneController>();
        }
        else
        {
            vehicle = null;
            carController = null;
        }
    }

    private void SpawnObstacle()
    {
        if (lanePositions.Count == 0 || obstaclePrefabs.Length == 0 || vehicle == null)
            return;

        Vector2 lane = lanePositions[Random.Range(0, lanePositions.Count)];

        GameObject selectedObstacle = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (selectedObstacle == null) return;

        Vector3 spawnPosition = new Vector3(
            lane.x,
            lane.y,
            vehicle.position.z + spawnDistance
        );

        Instantiate(selectedObstacle, spawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Vector3 centerPosition = (vehicle != null)
            ? new Vector3(vehicle.position.x, vehicle.position.y, vehicle.position.z + spawnDistance)
            : transform.position + transform.forward * spawnDistance;

        Gizmos.DrawWireCube(centerPosition, spawnAreaSize);
    }
}