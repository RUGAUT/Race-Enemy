using System.Threading;
using UnityEngine;

/// <summary>
/// Gestionnaire de zone asynchrone (Unity 6). 
/// Instancie les vagues et le boss sur des ancres spatiales valides.
/// </summary>
[DisallowMultipleComponent]
public class ZoneSpawnerManager : MonoBehaviour
{
    [Header("Configuration des Spawns")]
    [Tooltip("Liste de tous les prefabs de zombies standards")]
    [SerializeField] private GameObject[] standardZombiePrefabs;
    [SerializeField] private GameObject bossPrefab;
    [Tooltip("Points d'ancrage valides sur la carte (voies/hauteur)")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Paramètres de Zone")]
    [SerializeField] private float zoneDuration = 30f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnDistance = 50f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 50f);

    [Header("Debug & Gizmos")]
    [SerializeField] private Color gizmoColor = Color.blue;

    private Transform _vehicleTransform;
    private CancellationTokenSource _cts;

    private void Start()
    {
        // Résolution dynamique unique au démarrage (Zero GC en cours de partie)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _vehicleTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("[ZoneSpawnerManager] Véhicule introuvable ! Vérifiez le tag 'Player'.");
            return;
        }

        _cts = new CancellationTokenSource();
        _ = RunZoneSequenceAsync(_cts.Token);
    }

    private async Awaitable RunZoneSequenceAsync(CancellationToken token)
    {
        float timer = 0f;

        while (timer < zoneDuration)
        {
            if (token.IsCancellationRequested) return;

            SpawnZombie();

            // Remplacement des Coroutines : allocation nulle sur le tas (Heap)
            await Awaitable.WaitForSecondsAsync(spawnInterval, cancellationToken: token);
            timer += spawnInterval;
        }

        SpawnBoss();
    }

    private void SpawnZombie()
    {
        if (spawnPoints.Length == 0 || standardZombiePrefabs.Length == 0 || _vehicleTransform == null) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject selectedPrefab = standardZombiePrefabs[Random.Range(0, standardZombiePrefabs.Length)];

        Vector3 spawnPosition = new Vector3(
            spawnPoint.position.x,
            spawnPoint.position.y,
            _vehicleTransform.position.z + spawnDistance
        );

        Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || _vehicleTransform == null || spawnPoints.Length == 0) return;

        // Positionnement sur la voie centrale (déterministe)
        int centerIndex = spawnPoints.Length / 2;
        Transform referencePoint = spawnPoints[centerIndex];

        Vector3 bossSpawnPosition = new Vector3(
            referencePoint.position.x,
            referencePoint.position.y,
            _vehicleTransform.position.z + spawnDistance
        );

        // Instanciation simple. Le BossZombieController est désormais autonome 
        // et n'a plus besoin d'injection de dépendance pour suivre le joueur.
        Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        Vector3 centerPosition = (_vehicleTransform != null)
            ? new Vector3(_vehicleTransform.position.x, _vehicleTransform.position.y, _vehicleTransform.position.z + spawnDistance)
            : transform.position + transform.forward * spawnDistance;

        Gizmos.DrawWireCube(centerPosition, spawnAreaSize);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}