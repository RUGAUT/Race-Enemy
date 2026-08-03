using UnityEngine;

public class BossObstacle : MonoBehaviour
{
    [Header("Paramètres de l'Obstacle")]
    [SerializeField] private float forwardSpeed = 40f; // Vitesse vers le joueur (axe Z)
    [SerializeField] private float slideSpeed = 10f;   // Vitesse pour aller du boss vers la voie choisie (axe X)
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 5f;

    private float targetLaneX; // La voie cible où l'obstacle doit arriver

    public void Initialize(float targetX)
    {
        targetLaneX = targetX;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 1. Avance tout droit vers le joueur (vers l'arrière de la scène)
        transform.Translate(Vector3.back * forwardSpeed * Time.deltaTime, Space.World);

        // 2. Glisse progressivement de la position du boss vers la voie choisie sur l'axe X
        float newX = Mathf.MoveTowards(transform.position.x, targetLaneX, slideSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                vehicleHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}