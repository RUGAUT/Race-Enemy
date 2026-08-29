using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ZoneSpawnerManager : MonoBehaviour
{
    [Header("Configuration des Spawns")]
    [SerializeField] private GameObject[] standardZombiePrefabs;
    [Tooltip("Liste de tous les boss à vaincre dans l'ordre (ou sélectionnés)")]
    [SerializeField] private GameObject[] bossPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Paramètres de Zone")]
    [SerializeField] private float zoneDuration = 30f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnDistance = 50f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 2f, 50f);

    [Header("Interface")]
    [Tooltip("Le Slider UI qui indique la progression avant l'arrivée du Boss")]
    [SerializeField] private Slider zoneDurationSlider;

    [Header("Debug & Gizmos")]
    [SerializeField] private Color gizmoColor = Color.blue;

    private Transform _vehicleTransform;
    private CarLaneController _carController;
    private CancellationTokenSource _cts;

    private int currentBossIndex = 0;

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

        if (zoneDurationSlider != null) zoneDurationSlider.gameObject.SetActive(false);

        _cts = new CancellationTokenSource();
        _ = RunGameLoopAsync(_cts.Token);
    }

    private async Awaitable RunGameLoopAsync(CancellationToken token)
    {
        while (currentBossIndex < bossPrefabs.Length)
        {
            float timer = 0f;
            float nextSpawnTime = 0f;

            if (zoneDurationSlider != null)
            {
                zoneDurationSlider.gameObject.SetActive(true);
                zoneDurationSlider.maxValue = zoneDuration;
                zoneDurationSlider.value = 0f;
            }

            while (timer < zoneDuration)
            {
                if (token.IsCancellationRequested) return;

                if (timer >= nextSpawnTime)
                {
                    SpawnZombie();
                    nextSpawnTime += spawnInterval;
                }

                timer += Time.deltaTime;

                if (zoneDurationSlider != null)
                {
                    zoneDurationSlider.value = timer;
                }

                await Awaitable.NextFrameAsync(cancellationToken: token);
            }

            if (zoneDurationSlider != null)
            {
                zoneDurationSlider.value = zoneDuration;
            }

            await SpawnAndFightBossRoutine(token, bossPrefabs[currentBossIndex]);

            currentBossIndex++;
        }

        if (zoneDurationSlider != null)
        {
            zoneDurationSlider.gameObject.SetActive(false);
        }

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

    private async Awaitable SpawnAndFightBossRoutine(CancellationToken token, GameObject bossPrefabToSpawn)
    {
        if (bossPrefabToSpawn == null || _vehicleTransform == null || spawnPoints.Length == 0 || _carController == null) return;

        _carController.isStoppedForBoss = true;

        while (!_carController.IsFullyStopped)
        {
            if (token.IsCancellationRequested) return;
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        int centerIndex = spawnPoints.Length / 2;
        Transform referencePoint = spawnPoints[centerIndex];

        Vector3 bossSpawnPosition = new Vector3(
            referencePoint.position.x,
            referencePoint.position.y,
            _vehicleTransform.position.z + spawnDistance
        );

        GameObject spawnedBoss = Instantiate(bossPrefabToSpawn, bossSpawnPosition, Quaternion.identity);

        BossHealth bossHealth = spawnedBoss.GetComponent<BossHealth>();

        while (spawnedBoss != null)
        {
            if (token.IsCancellationRequested) return;
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        _carController.isStoppedForBoss = false;

        await Awaitable.WaitForSecondsAsync(2f, cancellationToken: token);
    }

    private void TriggerWinCondition()
    {
        Debug.Log("Tous les boss ont été vaincus ! Victoire !");

        // C'est ici que la magie opère : on passe le relais au GameManager !
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerWin();
        }
        else
        {
            Debug.LogWarning("Aucun GameManager trouvé dans la scène !");
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