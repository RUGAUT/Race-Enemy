using System.Collections.Generic;
using UnityEngine;

public class CustomRoadGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private int initialRoadCount = 5;
    // J'ai retiré le [SerializeField] private Transform player; car on le cherche automatiquement maintenant

    [SerializeField] private float roadLength = 30f;
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float despawnDistanceBehind = 20f;
    [SerializeField] private Vector3 spawnPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 roadRotation = Vector3.zero;

    private Queue<GameObject> activeRoads = new Queue<GameObject>();
    private float lastRoadZ;

    private Transform activePlayer; // Référence dynamique au joueur actuel
    private bool hasInitialized = false; // Pour savoir si on a déjà fait le spawn initial

    void Update()
    {
        // 1. Vérification : Si on n'a pas de joueur ou que le joueur actuel a été désactivé, on cherche le nouveau
        if (activePlayer == null || !activePlayer.gameObject.activeInHierarchy)
        {
            FindActivePlayer();
        }

        // 2. Sécurité : Si aucun véhicule "Player" n'est activé sur la scène, on met la génération en pause
        if (activePlayer == null) return;

        // 3. Initialisation : Ne se lance qu'une seule fois, dès qu'un véhicule est trouvé la première fois
        if (!hasInitialized)
        {
            lastRoadZ = activePlayer.position.z;
            for (int i = 0; i < initialRoadCount; i++)
            {
                SpawnRoad();
            }
            hasInitialized = true;
        }

        // 4. Génération continue devant le véhicule ACTIF
        if (activePlayer.position.z > lastRoadZ - spawnDistanceAhead)
        {
            SpawnRoad();
        }

        // 5. Suppression des routes loin derrière le véhicule ACTIF
        if (activeRoads.Count > 0 && activeRoads.Peek().transform.position.z < activePlayer.position.z - despawnDistanceBehind)
        {
            RemoveOldRoad();
        }
    }

    // Fonction qui cherche automatiquement le véhicule actif sur la scène
    private void FindActivePlayer()
    {
        // Assure-toi que tous tes véhicules ont bien le Tag "Player" !
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            activePlayer = playerObj.transform;
        }
    }

    private void SpawnRoad()
    {
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
        lastRoadZ = spawnPosition.z;
    }

    private void RemoveOldRoad()
    {
        if (activeRoads.Count > 0)
        {
            GameObject oldRoad = activeRoads.Dequeue();
            Destroy(oldRoad);
        }
    }

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