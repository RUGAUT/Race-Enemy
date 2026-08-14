using UnityEngine;

public class BossObstacle : MonoBehaviour
{
    [Header("Paramètres de l'Obstacle")]
    [SerializeField] private float forwardSpeed = 40f; // Vitesse vers le joueur (axe Z)
    [SerializeField] private float slideSpeed = 10f;   // Vitesse pour aller du boss vers la voie choisie (axe X)
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 5f;

    [Header("VFX & Impact")]
    [SerializeField] private GameObject impactVFX; // Le VFX à instancier (poussière, explosion...)
    [Tooltip("Le Tag de l'objet qui sert de route/sol")]
    [SerializeField] private string roadTag = "Road";

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
        // Si l'obstacle touche le joueur
        if (other.CompareTag("Player"))
        {
            VehicleHealth vehicleHealth = other.GetComponent<VehicleHealth>();
            if (vehicleHealth != null)
            {
                vehicleHealth.TakeDamage(damage);
            }

            // Optionnel : Jouer aussi le VFX si ça percute la voiture
            if (impactVFX != null)
            {
                Instantiate(impactVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
        // Si l'obstacle touche la route / le sol
        else if (other.CompareTag(roadTag))
        {
            if (impactVFX != null)
            {
                Instantiate(impactVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}