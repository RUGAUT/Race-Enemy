using UnityEngine;

// À placer sur le prefab du bonus "Tir Triple".
[RequireComponent(typeof(Collider))]
public class TripleShotPickup : MonoBehaviour
{
    [Header("Effet")]
    [SerializeField] private float duration = 6f;

    [Header("Animation")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Effets au ramassage")]
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSound;

    [Header("Nettoyage")]
    [Tooltip("Le bonus est détruit s'il se retrouve à cette distance derrière le joueur")]
    [SerializeField] private float destroyDistanceBehind = 20f;

    private Vector3 startPosition;
    private Transform player;
    private bool isCollected = false;

    private void Awake()
    {
        // Trigger pour que la voiture passe à travers
        GetComponent<Collider>().isTrigger = true;

        // Rigidbody cinématique pour garantir la détection du trigger
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Start()
    {
        startPosition = transform.position;
        FindPlayer();
    }

    private void Update()
    {
        // Rotation + flottement pour attirer l'œil
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        // Nettoyage si le joueur l'a dépassé
        if (player == null || !player.gameObject.activeInHierarchy) FindPlayer();
        if (player != null && transform.position.z < player.position.z - destroyDistanceBehind)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Accepte aussi un collider placé sur un enfant du véhicule
        bool isPlayer = other.CompareTag("Player") ||
                        (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        if (!isPlayer) return;

        isCollected = true;

        VehicleAttack.ActivateTripleShot(duration);

        if (pickupVFX != null) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = (playerObj != null) ? playerObj.transform : null;
    }
}