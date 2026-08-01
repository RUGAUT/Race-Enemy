using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// Contrôleur de Boss basé sur un système de points d'ancrage (Grid/Lanes).
/// Architecture idéale pour un Runner 3-Lanes : le Parent avance à vitesse constante, 
/// et le modèle 3D enfant navigue entre les points arrière (Repli) et avant (Attaque).
/// </summary>
[DisallowMultipleComponent]
public class BossZombieWaypointController : MonoBehaviour
{
    [Header("Hiérarchie (Structure du Prefab)")]
    [Tooltip("Le transform du modèle 3D du boss (l'enfant qui va se déplacer entre les points)")]
    [SerializeField] private Transform bossModel;
    [Tooltip("Les 3 points de repli à l'arrière (Gauche, Milieu, Droite)")]
    [SerializeField] private Transform[] backPoints = new Transform[3];
    [Tooltip("Les 3 points d'attaque à l'avant (Gauche, Milieu, Droite)")]
    [SerializeField] private Transform[] frontPoints = new Transform[3];

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private static readonly int AnimStateHash = Animator.StringToHash("State");

    [Header("Paramètres de Combat")]
    [SerializeField] private float moveSpeedBetweenPoints = 15f;
    [SerializeField] private float targetSpeed = 10f; // Doit être égale à la forwardSpeed du CarLaneController
    [SerializeField] private Vector2 timeBetweenAttacks = new Vector2(3f, 6f);
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private int attackDamage = 50;

    [Header("Détection d'attaque (Zero-GC)")]
    [SerializeField] private Vector3 attackHitboxSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private LayerMask playerLayerMask = ~0;

    private Transform _transform; // Cache du parent
    private Transform _playerTransform;
    private CancellationTokenSource _cts;

    // Buffer pré-alloué pour la physique
    private readonly Collider[] _hitResults = new Collider[2];

    private void Awake()
    {
        _transform = transform;
        if (animator == null && bossModel != null)
            animator = bossModel.GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        FindPlayer();

        _cts = new CancellationTokenSource();
        _ = RunWaypointCombatLoopAsync(_cts.Token);
    }

    private void FindPlayer()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;
    }

    private void Update()
    {
        // 1. Le conteneur parent (le GameObject vide) avance indéfiniment à la même vitesse que le véhicule.
        // Cela garantit que le quadrillage des 6 points reste toujours à la même distance relative du joueur.
        _transform.Translate(Vector3.forward * targetSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Machine à états asynchrone (Awaitable) gérant la navigation du modèle entre les points.
    /// </summary>
    private async Awaitable RunWaypointCombatLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // -- PHASE 1 : CROISIÈRE (Suivi de la voie du joueur depuis l'arrière) --
                SetAnimState(0); // Backwards
                float waitTimer = 0f;
                float waitDuration = UnityEngine.Random.Range(timeBetweenAttacks.x, timeBetweenAttacks.y);

                while (waitTimer < waitDuration)
                {
                    if (token.IsCancellationRequested) return;

                    waitTimer += Time.deltaTime;

                    // Le boss glisse doucement vers le point arrière correspondant à la voie actuelle du joueur
                    int currentLane = GetClosestLaneIndex();
                    MoveModelTowards(backPoints[currentLane].position, moveSpeedBetweenPoints * 0.5f);

                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }

                // -- PHASE 2 : CHARGE (Déplacement vers le point avant) --
                SetAnimState(1); // Running
                int targetAttackLane = GetClosestLaneIndex(); // Verrouille la voie pour l'attaque

                while (Vector3.Distance(bossModel.position, frontPoints[targetAttackLane].position) > 0.1f)
                {
                    if (token.IsCancellationRequested) return;

                    MoveModelTowards(frontPoints[targetAttackLane].position, moveSpeedBetweenPoints);
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }

                // -- PHASE 3 : ATTAQUE --
                SetAnimState(2); // Attack
                ApplyAttackDamage(frontPoints[targetAttackLane].position);
                await Awaitable.WaitForSecondsAsync(attackDuration, cancellationToken: token);

                // -- PHASE 4 : REPLI (Retour au point arrière) --
                SetAnimState(0); // Backwards (ou 1 si tu as une animation de fuite)
                while (Vector3.Distance(bossModel.position, backPoints[targetAttackLane].position) > 0.1f)
                {
                    if (token.IsCancellationRequested) return;

                    MoveModelTowards(backPoints[targetAttackLane].position, moveSpeedBetweenPoints);
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
            }
        }
        catch (OperationCanceledException) { /* Arrêt propre */ }
    }

    /// <summary>
    /// Déplace le modèle 3D du boss vers une cible sans toucher à la position du parent.
    /// </summary>
    private void MoveModelTowards(Vector3 targetPosition, float speed)
    {
        if (bossModel == null) return;
        bossModel.position = Vector3.MoveTowards(bossModel.position, targetPosition, speed * Time.deltaTime);

        // Assure que le modèle regarde toujours vers le joueur/l'avant du parent
        bossModel.rotation = Quaternion.LookRotation(Vector3.back);
    }

    /// <summary>
    /// Détermine dynamiquement sur quelle voie (0, 1, 2) se trouve le joueur 
    /// en comparant sa position X avec les points arrière.
    /// </summary>
    private int GetClosestLaneIndex()
    {
        if (_playerTransform == null || backPoints.Length < 3) return 1;

        float minDistance = float.MaxValue;
        int closestIndex = 1; // Voie du milieu par défaut

        for (int i = 0; i < backPoints.Length; i++)
        {
            if (backPoints[i] == null) continue;

            float dist = Mathf.Abs(_playerTransform.position.x - backPoints[i].position.x);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private void ApplyAttackDamage(Vector3 attackCenter)
    {
        int hitCount = Physics.OverlapBoxNonAlloc(attackCenter, attackHitboxSize * 0.5f, _hitResults, Quaternion.identity, playerLayerMask);

        for (int i = 0; i < hitCount; i++)
        {
            if (_hitResults[i] != null && _hitResults[i].CompareTag("Player"))
            {
                if (_hitResults[i].TryGetComponent<VehicleHealth>(out var vehicleHealth))
                {
                    vehicleHealth.TakeDamage(attackDamage);
                }
                break;
            }
        }
    }

    private void SetAnimState(int stateIndex)
    {
        if (animator != null) animator.SetInteger(AnimStateHash, stateIndex);
    }

    private void OnDrawGizmos()
    {
        if (frontPoints == null || backPoints == null) return;

        // Visualisation des points et de la zone d'attaque dans l'éditeur
        for (int i = 0; i < frontPoints.Length; i++)
        {
            if (frontPoints[i] != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Rouge pour l'attaque
                Gizmos.DrawWireCube(frontPoints[i].position, attackHitboxSize);
            }
            if (backPoints.Length > i && backPoints[i] != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Vert pour le repli
                Gizmos.DrawWireSphere(backPoints[i].position, 0.5f);
            }
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}