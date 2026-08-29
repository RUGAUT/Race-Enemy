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

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            vehicle = playerObj.transform;
            carController = playerObj.GetComponent<CarLaneController>();
        }

        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // On vérifie si la voiture est arrêtée pour le boss
            // Si c'est le cas, on passe notre tour et on ne fait rien
            if (carController != null && carController.isStoppedForBoss)
            {
                continue;
            }

            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        if (spawnPoints.Length == 0 || obstaclePrefabs.Length == 0 || vehicle == null)
            return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        int randomObstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedObstacle = obstaclePrefabs[randomObstacleIndex];

        Vector3 spawnPosition = new Vector3(
            spawnPoint.position.x,
            spawnPoint.position.y,
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