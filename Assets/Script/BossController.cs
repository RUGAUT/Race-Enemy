using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Composants")]
    [SerializeField] private Animator animator;

    [Header("Voies (Lanes)")]
    [SerializeField] private float[] lanePositions = new float[] { -2.0f, 0.0f, 2.0f };

    [Header("Mise en scène (Entrée & Cri)")]
    [SerializeField] private float combatDistance = 30f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float screamDuration = 2f;

    [Header("Pattern 1 (Charge)")]
    [SerializeField] private float chargeSpeed = 30f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float distanceBehindPlayer = 40f;

    [Header("Pattern 2 (Lancer d'Obstacles)")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int obstaclesToThrow = 3;
    [SerializeField] private float timeBetweenThrows = 1.5f;

    [Header("Pattern 3 (Appel de Zombies)")]
    [Tooltip("Liste des prefabs de zombies que le boss peut appeler")]
    [SerializeField] private GameObject[] minionZombiePrefabs;
    [Tooltip("Nombre de zombies invoqués lors du cri")]
    [SerializeField] private int zombiesToSpawnCount = 3;

    [Header("Dégâts du Boss (Contact)")]
    [SerializeField] private int bossDamage = 50;

    private Transform player;
    private bool isAttacking = false;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Le Boss ne trouve pas le joueur !");
            return;
        }

        StartCoroutine(BossSequenceManager());
    }

    private IEnumerator BossSequenceManager()
    {
        yield return StartCoroutine(EntranceCinematic());
        yield return new WaitForSeconds(1f);

        while (true)
        {
            yield return StartCoroutine(PlayScream());

            // --- MIS À JOUR : 3 PATTERNS ALÉATOIRES ---
            // Random.Range(1, 4) peut donner 1, 2 ou 3
            int randomPattern = Random.Range(1, 4);

            if (randomPattern == 1)
            {
                yield return StartCoroutine(Pattern1_ChargeAndReturn());
            }
            else if (randomPattern == 2)
            {
                yield return StartCoroutine(Pattern2_ThrowObstacles());
            }
            else if (randomPattern == 3)
            {
                yield return StartCoroutine(Pattern3_CallZombies());
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    private IEnumerator EntranceCinematic()
    {
        animator.SetBool("IsWalking", true);

        while (true)
        {
            Vector3 targetPosition = new Vector3(lanePositions[1], transform.position.y, player.position.z + combatDistance);
            transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f) break;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("IsWalking", false);
    }

    private IEnumerator PlayScream()
    {
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
        animator.SetTrigger("Scream");
        yield return new WaitForSeconds(screamDuration);
    }

    private IEnumerator Pattern1_ChargeAndReturn()
    {
        isAttacking = true;
        float targetX = lanePositions[Random.Range(0, lanePositions.Length)];

        animator.SetBool("IsRunning", true);

        while (true)
        {
            Vector3 targetChargePosition = new Vector3(targetX, transform.position.y, player.position.z - distanceBehindPlayer);
            transform.LookAt(targetChargePosition);

            if (Vector3.Distance(transform.position, targetChargePosition) < 0.5f) break;

            transform.position = Vector3.MoveTowards(transform.position, targetChargePosition, chargeSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("IsRunning", false);
        yield return new WaitForSeconds(0.5f);

        animator.SetBool("IsRunning", true);

        float returnTargetX = targetX;
        while (returnTargetX == targetX)
        {
            returnTargetX = lanePositions[Random.Range(0, lanePositions.Length)];
        }

        while (true)
        {
            Vector3 targetReturnPosition = new Vector3(returnTargetX, transform.position.y, player.position.z + combatDistance);
            transform.LookAt(targetReturnPosition);

            if (Vector3.Distance(transform.position, targetReturnPosition) < 0.5f) break;

            transform.position = Vector3.MoveTowards(transform.position, targetReturnPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("IsRunning", false);
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

        isAttacking = false;
    }

    private IEnumerator Pattern2_ThrowObstacles()
    {
        isAttacking = true;
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

        for (int i = 0; i < obstaclesToThrow; i++)
        {
            animator.SetTrigger("Throw");
            yield return new WaitForSeconds(timeBetweenThrows);
        }

        isAttacking = false;
    }

    // --- NOUVEAU : PATTERN 3 (APPEL DE ZOMBIES) ---
    private IEnumerator Pattern3_CallZombies()
    {
        isAttacking = true;
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

        // Le boss lève les bras ou crie pour appeler ses sbires
        animator.SetTrigger("Scream"); // On réutilise le trigger Scream ou un trigger spécifique "Call"

        // Fait apparaître plusieurs zombies répartis sur les voies
        if (minionZombiePrefabs != null && minionZombiePrefabs.Length > 0)
        {
            for (int i = 0; i < zombiesToSpawnCount; i++)
            {
                // Choisit une voie aléatoire
                float randomLaneX = lanePositions[Random.Range(0, lanePositions.Length)];

                // Choisit un type de zombie aléatoire dans la liste
                GameObject randomZombiePrefab = minionZombiePrefabs[Random.Range(0, minionZombiePrefabs.Length)];

                // Position d'apparition un peu plus loin devant le joueur pour qu'il ait le temps de les voir venir
                Vector3 spawnPosition = new Vector3(randomLaneX, transform.position.y, player.position.z + combatDistance + 10f);

                Instantiate(randomZombiePrefab, spawnPosition, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("Aucun prefab de zombie minion assigné pour le Pattern 3 !");
        }

        // Attend la fin de l'action
        yield return new WaitForSeconds(screamDuration);

        isAttacking = false;
    }

    public void ThrowObstacleEvent()
    {
        if (obstaclePrefab != null)
        {
            float randomLaneX = lanePositions[Random.Range(0, lanePositions.Length)];
            Vector3 spawnPosition = transform.position + new Vector3(0f, 1.5f, -0.5f);
            GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);

            BossObstacle obstacleScript = newObstacle.GetComponent<BossObstacle>();
            if (obstacleScript != null)
            {
                obstacleScript.Initialize(randomLaneX);
            }
        }
        else
        {
            Debug.LogWarning("Aucun Prefab d'obstacle assigné dans l'inspecteur du Boss !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null) vehicleHealth.TakeDamage(bossDamage);
        }
    }
}