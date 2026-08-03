using System.Collections;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class ZoneSpawnerManager : MonoBehaviour
{
    [Header("Configuration des Spawns")]
    [SerializeField] private GameObject[] standardZombiePrefabs;
    [Tooltip("Liste de tous les boss à vaincre dans l'ordre (ou sélectionnés)")]
    [SerializeField] private GameObject[] bossPrefabs; // --- MODIFIÉ : Tableau de boss ---
    [SerializeField] private Transform[] spawnPoints;

    [Header("Paramètres de Zone")]
    [SerializeField] private float zoneDuration = 30f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnDistance = 50f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 50f);

    [Header("Interface de Victoire")]
    [SerializeField] private GameObject winPanel; // --- NOUVEAU : Panel Win à afficher à la fin ---

    [Header("Debug & Gizmos")]
    [SerializeField] private Color gizmoColor = Color.blue;

    private Transform _vehicleTransform;
    private CarLaneController _carController;
    private CancellationTokenSource _cts;

    private int currentBossIndex = 0; // Suit quel boss doit apparaître

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _vehicleTransform = playerObj.transform;
            _carController = playerObj.GetComponent<CarLaneController>();
        }
        else
        {
            Debug.LogError("[ZoneSpawnerManager] Véhicule introuvable !");
            return;
        }

        if (winPanel != null) winPanel.SetActive(false);

        _cts = new CancellationTokenSource();
        _ = RunGameLoopAsync(_cts.Token);
    }

    /// <summary>
    /// Boucle principale du jeu : Vagues de zombies -> Boss -> Répétition ou Victoire
    /// </summary>
    private async Awaitable RunGameLoopAsync(CancellationToken token)
    {
        // Tant qu'il reste des boss à affronter
        while (currentBossIndex < bossPrefabs.Length)
        {
            // --- ÉTAPE 1 : VAGUES DE ZOMBIES ---
            float timer = 0f;
            while (timer < zoneDuration)
            {
                if (token.IsCancellationRequested) return;

                SpawnZombie();

                await Awaitable.WaitForSecondsAsync(spawnInterval, cancellationToken: token);
                timer += spawnInterval;
            }

            // --- ÉTAPE 2 : SPAWN ET COMBAT DU BOSS ACTUEL ---
            // On attend que le boss actuel soit complètement vaincu avant de continuer
            await SpawnAndFightBossRoutine(token, bossPrefabs[currentBossIndex]);

            // Passe au boss suivant pour la prochaine boucle
            currentBossIndex++;

            // S'il reste encore un boss, la boucle continue et réactive les zombies
        }

        // --- ÉTAPE 3 : CONDITION DE VICTOIRE FINALE ---
        TriggerWinCondition();
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

    /// <summary>
    /// Gère l'arrêt du véhicule, l'apparition du boss, et attend sa mort pour rendre la main
    /// </summary>
    private async Awaitable SpawnAndFightBossRoutine(CancellationToken token, GameObject bossPrefabToSpawn)
    {
        if (bossPrefabToSpawn == null || _vehicleTransform == null || spawnPoints.Length == 0 || _carController == null) return;

        // 1. Freinage progressif du véhicule
        _carController.isStoppedForBoss = true;

        while (!_carController.IsFullyStopped)
        {
            if (token.IsCancellationRequested) return;
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        // 2. Instanciation du Boss actuel
        int centerIndex = spawnPoints.Length / 2;
        Transform referencePoint = spawnPoints[centerIndex];

        Vector3 bossSpawnPosition = new Vector3(
            referencePoint.position.x,
            referencePoint.position.y,
            _vehicleTransform.position.z + spawnDistance
        );

        GameObject spawnedBoss = Instantiate(bossPrefabToSpawn, bossSpawnPosition, Quaternion.identity);

        // 3. Attente active de la mort du Boss
        BossHealth bossHealth = spawnedBoss.GetComponent<BossHealth>();

        // Si le boss a un script de vie, on attend qu'il soit détruit ou que sa vie tombe à 0
        while (spawnedBoss != null)
        {
            if (token.IsCancellationRequested) return;
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        // 4. Le boss est vaincu : le véhicule se remet à avancer pour la prochaine zone de zombies
        _carController.isStoppedForBoss = false;

        // Petite pause de transition avant de relancer les zombies
        await Awaitable.WaitForSecondsAsync(2f, cancellationToken: token);
    }

    private void TriggerWinCondition()
    {
        Debug.Log("Tous les boss ont été vaincus ! Victoire !");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f; // Fige le jeu sur l'écran de victoire
        }
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