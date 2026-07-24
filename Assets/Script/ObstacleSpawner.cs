using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs; // Tableau de prefabs pour différents types d'obstacles
    [SerializeField] private Transform[] spawnPoints; // Les points de spawn sur la route
    [SerializeField] private float spawnInterval = 2f; // Temps entre chaque spawn
    [SerializeField] private float spawnDistance = 50f; // Distance devant le véhicule où apparaissent les obstacles
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 50f); // Taille de la zone de spawn

    private Transform vehicle; // Référence au véhicule

    private void Start()
    {
        vehicle = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(SpawnObstacles());
    }

    private IEnumerator SpawnObstacles()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        // Sélectionnez un point de spawn aléatoire
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // Sélectionnez un obstacle aléatoire dans le tableau obstaclePrefabs
        int randomObstacleIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject selectedObstacle = obstaclePrefabs[randomObstacleIndex];

        // Spawner l'obstacle à une certaine distance devant le véhicule
        Vector3 spawnPosition = new Vector3(spawnPoint.position.x, spawnPoint.position.y, vehicle.position.z + spawnDistance);
        Instantiate(selectedObstacle, spawnPosition, Quaternion.identity);
    }

    // Méthode pour dessiner les gizmos
    private void OnDrawGizmos()
    {
        // Définir la couleur du gizmo
        Gizmos.color = Color.blue;

        // Dessiner un cube pour représenter la zone de spawn
        if (vehicle != null)
        {
            Gizmos.DrawWireCube(new Vector3(vehicle.position.x, vehicle.position.y, vehicle.position.z + spawnDistance), spawnAreaSize);
        }
    }
}
