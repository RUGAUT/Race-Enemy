using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Paramètres de Déplacement")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Axe/Direction dans lequel le zombie SE DÉPLACE (espace monde)")]
    [SerializeField] private Vector3 moveDirection = Vector3.back; // Ex: (0, 0, -1) pour -Z

    [Tooltip("Axe/Direction dans lequel le zombie REGARDE (espace monde)")]
    [SerializeField] private Vector3 lookDirection = Vector3.back; // Ex: (0, 0, -1) ou tout autre axe

    [Header("Paramètres de Collision")]
    [SerializeField] private int pointsValue = 1;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private int crashDamage = 20;

    [Header("VFX")]
    [SerializeField] private GameObject vfxPrefab;

    private Health health;

    private void Start()
    {
        health = GetComponent<Health>();

        // Forcer la rotation au lancement du jeu
        AlignLookRotation();
    }

    private void Update()
    {
        // Déplacement indépendant dans la direction 'moveDirection'
        if (moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void AlignLookRotation()
    {
        // Applique la rotation selon la direction 'lookDirection'
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
        }
    }

    // Permet de voir le résultat de 'lookDirection' en temps réel dans l'Éditeur Unity !
    private void OnValidate()
    {
        AlignLookRotation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                vehicleHealth.TakeDamage(crashDamage);
            }

            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddZombieScore(pointsValue);
            }

            gameObject.SetActive(false);
            InstantiateVFX();
            Invoke(nameof(DestroyZombie), destroyDelay);
        }
    }

    private void InstantiateVFX()
    {
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    private void DestroyZombie()
    {
        Destroy(gameObject);
    }
}