using UnityEngine;

public class CarLaneController : MonoBehaviour
{
    // Positions des 3 voies (ajuste ces valeurs en fonction de la largeur de la route)
    private float[] lanePositions = new float[] { -2.0f, 0.0f, 2.0f }; // Gauche, Milieu, Droite
    private int currentLane = 1; // Commence au milieu (index 1)

    // Vitesse du changement de voie et vitesse vers l'avant
    public float laneChangeSpeed = 5.0f;  // Vitesse de changement de voie
    public float forwardSpeed = 10.0f;    // Vitesse à laquelle le véhicule avance

    private Vector3 targetPosition;       // Position cible de la voie après le changement
    private Vector2 startTouchPosition;   // Position de départ du toucher
    private bool isSwiping = false;       // Indicateur de swipe

    void Start()
    {
        // Position initiale du véhicule (commence au milieu)
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void Update()
    {
        // Mouvement constant vers l'avant
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);

        // Déplacement latéral en douceur vers la position cible
        Vector3 newPosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, laneChangeSpeed * Time.deltaTime);

        // Gestion du toucher (glissement)
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Début du toucher (enregistre la position initiale)
                startTouchPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping)
            {
                // Si le joueur fait glisser, on vérifie le mouvement horizontal
                Vector2 touchDeltaPosition = touch.deltaPosition;

                if (touchDeltaPosition.x > 50) // Si le glissement est vers la droite
                {
                    if (currentLane < 2) // Si le véhicule n'est pas déjà à droite
                    {
                        currentLane++;
                        ChangeLane();
                    }
                    isSwiping = false; // Fin du swipe pour éviter de continuer à bouger
                }
                else if (touchDeltaPosition.x < -50) // Si le glissement est vers la gauche
                {
                    if (currentLane > 0) // Si le véhicule n'est pas déjà à gauche
                    {
                        currentLane--;
                        ChangeLane();
                    }
                    isSwiping = false;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // Fin du toucher
                isSwiping = false;
            }
        }
    }

    // Méthode pour initier le changement de voie
    void ChangeLane()
    {
        // Mettre à jour la position cible en fonction de la nouvelle voie
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }
}
