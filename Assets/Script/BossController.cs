using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Composants")]
    [SerializeField] private Animator animator;
    private CameraFollow cameraFollow;

    [Header("Patterns Actifs")]
    [Tooltip("Coche ou décoche pour activer les attaques spécifiques à ce Boss")]
    [SerializeField] private bool enablePattern1_Charge = true;
    [SerializeField] private bool enablePattern2_Throw = true;
    [SerializeField] private bool enablePattern3_Zombies = true;
    [SerializeField] private bool enablePattern4_Jump = true;

    [Header("Voies (Lanes)")]
    [SerializeField] private float[] lanePositions = new float[] { -2.0f, 0.0f, 2.0f };

    [Header("Mise en scène (Entrée & Cri)")]
    [SerializeField] private float combatDistance = 30f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float screamDuration = 2f;

    [Header("Effets Caméra (Shake)")]
    [SerializeField] private float screamShakeDuration = 1.5f;
    [SerializeField] private float screamShakeMagnitude = 0.3f;

    [Header("Pattern 1 (Charge)")]
    [SerializeField] private float chargeSpeed = 30f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float distanceBehindPlayer = 40f;

    [Header("Pattern 2 (Lancer d'Obstacles)")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int obstaclesToThrow = 3;
    [SerializeField] private float timeBetweenThrows = 1.5f;

    [Header("Pattern 3 (Appel de Zombies)")]
    [SerializeField] private GameObject[] minionZombiePrefabs;
    [SerializeField] private int zombiesToSpawnCount = 3;

    [Header("Pattern 4 (Saut Écrasant)")]
    [SerializeField] private float jumpHeight = 25f;
    [SerializeField] private float jumpUpSpeed = 40f;
    [SerializeField] private float fallDownSpeed = 80f;
    [SerializeField] private float hangTime = 1f;
    [SerializeField] private float landingShakeDuration = 0.8f;
    [SerializeField] private float landingShakeMagnitude = 0.6f;
    [SerializeField] private GameObject landingVFX;

    [Header("Dégâts du Boss (Contact)")]
    [SerializeField] private int bossDamage = 50;

    private Transform player;
    private bool isAttacking = false;
    private bool readyToJump = false; // Bloque le saut jusqu'à l'Animation Event

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        cameraFollow = FindFirstObjectByType<CameraFollow>();

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

            List<int> availablePatterns = new List<int>();

            if (enablePattern1_Charge) availablePatterns.Add(1);
            if (enablePattern2_Throw) availablePatterns.Add(2);
            if (enablePattern3_Zombies) availablePatterns.Add(3);
            if (enablePattern4_Jump) availablePatterns.Add(4);

            if (availablePatterns.Count > 0)
            {
                int randomIndex = Random.Range(0, availablePatterns.Count);
                int chosenPattern = availablePatterns[randomIndex];

                if (chosenPattern == 1)
                {
                    yield return StartCoroutine(Pattern1_ChargeAndReturn());
                }
                else if (chosenPattern == 2)
                {
                    yield return StartCoroutine(Pattern2_ThrowObstacles());
                }
                else if (chosenPattern == 3)
                {
                    yield return StartCoroutine(Pattern3_CallZombies());
                }
                else if (chosenPattern == 4)
                {
                    yield return StartCoroutine(Pattern4_JumpAndSmash());
                }
            }
            else
            {
                Debug.LogWarning("Aucun pattern n'est activé sur ce Boss !");
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

        if (cameraFollow != null) cameraFollow.TriggerShake(screamShakeDuration, screamShakeMagnitude);

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

    private IEnumerator Pattern3_CallZombies()
    {
        isAttacking = true;
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));

        animator.SetTrigger("Scream");

        if (cameraFollow != null) cameraFollow.TriggerShake(screamShakeDuration, screamShakeMagnitude);

        if (minionZombiePrefabs != null && minionZombiePrefabs.Length > 0)
        {
            for (int i = 0; i < zombiesToSpawnCount; i++)
            {
                float randomLaneX = lanePositions[Random.Range(0, lanePositions.Length)];
                GameObject randomZombiePrefab = minionZombiePrefabs[Random.Range(0, minionZombiePrefabs.Length)];
                Vector3 spawnPosition = new Vector3(randomLaneX, transform.position.y, player.position.z + combatDistance + 10f);
                Instantiate(randomZombiePrefab, spawnPosition, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(screamDuration);

        isAttacking = false;
    }

    private IEnumerator Pattern4_JumpAndSmash()
    {
        isAttacking = true;
        float originalY = transform.position.y;

        readyToJump = false;
        animator.SetTrigger("Jump");

        while (!readyToJump)
        {
            yield return null;
        }

        Vector3 peakPosition = new Vector3(transform.position.x, originalY + jumpHeight, transform.position.z);
        while (transform.position.y < peakPosition.y - 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, peakPosition, jumpUpSpeed * Time.deltaTime);
            yield return null;
        }

        float targetX = lanePositions[Random.Range(0, lanePositions.Length)];
        Vector3 hoverPosition = new Vector3(targetX, peakPosition.y, player.position.z + 2f);
        transform.position = hoverPosition;

        yield return new WaitForSeconds(hangTime);

        Vector3 landingPosition = new Vector3(targetX, originalY, player.position.z + 2f);
        while (transform.position.y > landingPosition.y + 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, landingPosition, fallDownSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = landingPosition;

        animator.SetTrigger("Land");
        if (cameraFollow != null) cameraFollow.TriggerShake(landingShakeDuration, landingShakeMagnitude);
        if (landingVFX != null) Instantiate(landingVFX, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);

        animator.SetBool("IsRunning", true);
        float returnTargetX = lanePositions[1];

        while (true)
        {
            Vector3 targetReturnPosition = new Vector3(returnTargetX, originalY, player.position.z + combatDistance);
            transform.LookAt(targetReturnPosition);

            if (Vector3.Distance(transform.position, targetReturnPosition) < 0.5f) break;

            transform.position = Vector3.MoveTowards(transform.position, targetReturnPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        animator.SetBool("IsRunning", false);
        transform.LookAt(new Vector3(player.position.x, originalY, player.position.z));

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
    }

    public void JumpUpEvent()
    {
        readyToJump = true;
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