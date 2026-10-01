using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Zombie kamikaze :
// 1. Il avance lentement comme un zombie normal.
// 2. Quand le joueur approche, il s'arrête, tremble et clignote en rouge (avertissement).
// 3. Il charge à toute vitesse en se recalant sur la voie du joueur.
// 4. Au contact : il explose et inflige de gros dégâts au véhicule.
//    S'il est tué avant (balle / grenade) : il explose et blesse les zombies autour de lui.
//
// À utiliser À LA PLACE du script Zombie (pas les deux sur le même prefab).
// Nécessite ton script Health sur le même objet.
[RequireComponent(typeof(Health))]
public class KamikazeZombie : MonoBehaviour
{
    private enum State { Walking, Warning, Charging, Exploded }

    [Header("=== Déplacement ===")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float chargeSpeed = 18f;
    [Tooltip("Direction de déplacement dans le monde (comme ton script Zombie)")]
    [SerializeField] private Vector3 moveDirection = Vector3.back;

    [Header("=== Déclenchement de la charge ===")]
    [Tooltip("Distance (sur l'axe Z) à laquelle le zombie repère le joueur")]
    [SerializeField] private float detectionDistance = 25f;
    [Tooltip("Durée de l'avertissement (tremblement + clignotement) avant la charge")]
    [SerializeField] private float warningDuration = 0.7f;
    [Tooltip("Vitesse à laquelle il se recale sur la voie du joueur pendant la charge (0 = fonce tout droit)")]
    [SerializeField] private float laneTrackingSpeed = 2f;

    [Header("=== Explosion au contact du véhicule ===")]
    [SerializeField] private int contactDamage = 35;

    [Header("=== Explosion de zone ===")]
    [SerializeField] private float explosionRadius = 4f;
    [Tooltip("Dégâts infligés aux autres zombies dans le rayon")]
    [SerializeField] private int explosionDamage = 100;
    [Tooltip("Si coché, l'explosion à la mort peut aussi blesser le véhicule s'il est trop proche")]
    [SerializeField] private bool damagePlayerOnDeath = false;
    [SerializeField] private int playerDeathExplosionDamage = 15;
    [SerializeField] private bool destroyObstacles = true;
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("=== Effets ===")]
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float shakeDuration = 0.4f;
    [SerializeField] private float shakeMagnitude = 0.4f;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float blinkInterval = 0.08f;
    [Tooltip("Amplitude du tremblement pendant l'avertissement")]
    [SerializeField] private float warningShakeAmount = 0.08f;

    [Header("=== Animation (optionnel) ===")]
    [SerializeField] private Animator animator;
    [Tooltip("Nom du paramètre Bool de l'Animator activé pendant la charge (laisser vide si aucun)")]
    [SerializeField] private string chargeBoolName = "IsRunning";

    [Header("=== Nettoyage ===")]
    [Tooltip("Le zombie est détruit s'il se retrouve à cette distance derrière le joueur")]
    [SerializeField] private float destroyDistanceBehind = 20f;

    private State state = State.Walking;
    private Health health;
    private Transform player;
    private CameraFollow cameraFollow;
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        health = GetComponent<Health>();
        health.OnDeath += OnKilled;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        cameraFollow = FindFirstObjectByType<CameraFollow>();
        FindPlayer();

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection.normalized);
        }
    }

    private void OnDestroy()
    {
        if (health != null) health.OnDeath -= OnKilled;
    }

    private void Update()
    {
        if (state == State.Exploded) return;

        if (player == null || !player.gameObject.activeInHierarchy) FindPlayer();

        switch (state)
        {
            case State.Walking:
                transform.Translate(moveDirection.normalized * walkSpeed * Time.deltaTime, Space.World);

                if (player != null)
                {
                    float distanceToPlayer = transform.position.z - player.position.z;
                    if (distanceToPlayer > 0f && distanceToPlayer < detectionDistance)
                    {
                        StartCoroutine(WarningThenCharge());
                    }
                }
                break;

            case State.Charging:
                ChargeMovement();
                break;
        }

        // Nettoyage si le joueur l'a dépassé
        if (player != null && transform.position.z < player.position.z - destroyDistanceBehind)
        {
            Destroy(gameObject);
        }
    }

    // --- Phase d'avertissement : le joueur a le temps de réagir ---
    private IEnumerator WarningThenCharge()
    {
        state = State.Warning;
        StartCoroutine(BlinkRoutine());

        Vector3 basePosition = transform.position;
        float timer = 0f;

        while (timer < warningDuration)
        {
            if (state == State.Exploded) yield break;

            timer += Time.deltaTime;

            // Petit tremblement sur place
            Vector3 jitter = new Vector3(
                Random.Range(-warningShakeAmount, warningShakeAmount),
                0f,
                Random.Range(-warningShakeAmount, warningShakeAmount)
            );
            transform.position = basePosition + jitter;

            yield return null;
        }

        transform.position = basePosition;

        if (state == State.Exploded) yield break;

        state = State.Charging;
        SetAnimatorBool(chargeBoolName, true);
    }

    private void ChargeMovement()
    {
        Vector3 position = transform.position;

        // Avance vers le joueur
        position += moveDirection.normalized * chargeSpeed * Time.deltaTime;

        // Se recale progressivement sur la voie du joueur (esquivable si on change de voie au bon moment)
        if (player != null && laneTrackingSpeed > 0f)
        {
            position.x = Mathf.MoveTowards(position.x, player.position.x, laneTrackingSpeed * Time.deltaTime);
        }

        Vector3 movement = position - transform.position;
        transform.position = position;

        if (movement.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(movement.normalized);
        }
    }

    // --- Clignotement rouge (continue pendant la charge) ---
    private IEnumerator BlinkRoutine()
    {
        bool isRed = false;

        while (state != State.Exploded)
        {
            isRed = !isRed;
            SetTint(isRed);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SetTint(bool active)
    {
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            if (active)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", warningColor); // URP
                propertyBlock.SetColor("_Color", warningColor);     // Built-in
                rend.SetPropertyBlock(propertyBlock);
            }
            else
            {
                rend.SetPropertyBlock(null); // Remet la couleur d'origine
            }
        }
    }

    // --- Contact avec le véhicule ---
    private void OnTriggerEnter(Collider other)
    {
        if (state == State.Exploded) return;

        bool isPlayer = other.CompareTag("Player") ||
                        (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        if (!isPlayer) return;

        VehicleHealth vehicleHealth = other.GetComponentInParent<VehicleHealth>();
        if (vehicleHealth != null)
        {
            // Si la barrière est active, VehicleHealth ignore déjà les dégâts
            vehicleHealth.TakeDamage(contactDamage);
        }

        Explode(fromContact: true);
        Destroy(gameObject);
    }

    // --- Appelée par Health quand le zombie meurt (balle, grenade, autre explosion...) ---
    private void OnKilled()
    {
        if (state == State.Exploded) return;
        Explode(fromContact: false);
        // Pas besoin de Destroy ici : ton script Health s'en charge déjà
    }

    private void Explode(bool fromContact)
    {
        state = State.Exploded;

        if (explosionVFX != null) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        if (cameraFollow != null) cameraFollow.TriggerShake(shakeDuration, shakeMagnitude);

        // Évite de toucher deux fois la même cible si elle a plusieurs colliders
        HashSet<GameObject> alreadyHit = new HashSet<GameObject>();
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            // Obstacles
            if (destroyObstacles && hit.CompareTag(obstacleTag))
            {
                Destroy(hit.gameObject);
                continue;
            }

            // Autres zombies (réaction en chaîne possible avec d'autres kamikazes !)
            Health otherHealth = hit.GetComponentInParent<Health>();
            if (otherHealth != null && otherHealth != health)
            {
                if (alreadyHit.Add(otherHealth.gameObject)) otherHealth.TakeDamage(explosionDamage);
                continue;
            }

            // Boss
            BossHealth bossHealth = hit.GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                if (alreadyHit.Add(bossHealth.gameObject)) bossHealth.TakeDamage(explosionDamage);
                continue;
            }

            // Véhicule (uniquement pour l'explosion à la mort, le contact a déjà infligé ses dégâts)
            if (!fromContact && damagePlayerOnDeath)
            {
                VehicleHealth vehicleHealth = hit.GetComponentInParent<VehicleHealth>();
                if (vehicleHealth != null && alreadyHit.Add(vehicleHealth.gameObject))
                {
                    vehicleHealth.TakeDamage(playerDeathExplosionDamage);
                }
            }
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName)) return;

        // Vérifie que le paramètre existe pour éviter les warnings dans la console
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName && param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parameterName, value);
                return;
            }
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = (playerObj != null) ? playerObj.transform : null;
    }

    private void OnDrawGizmosSelected()
    {
        // Rayon d'explosion
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // Distance de détection
        Gizmos.color = Color.yellow;
        Vector3 direction = moveDirection == Vector3.zero ? Vector3.back : moveDirection.normalized;
        Gizmos.DrawLine(transform.position, transform.position + direction * detectionDistance);
    }
}