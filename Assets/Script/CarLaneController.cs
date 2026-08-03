using UnityEngine;

public class CarLaneController : MonoBehaviour
{
    private float[] lanePositions = new float[] { -2.0f, 0.0f, 2.0f };
    private int currentLane = 1;

    [Header("Vitesse et Mouvement")]
    public float laneChangeSpeed = 5.0f;
    public float forwardSpeed = 10.0f;
    public float brakingSpeed = 5.0f;

    [Header("État du Combat")]
    public bool isStoppedForBoss = false;

    private float currentForwardSpeed;
    private Vector3 targetPosition;
    private Vector2 startTouchPosition;
    private bool isSwiping = false;

    // --- NOUVEAU : Le spawner va lire ça pour savoir s'il peut spawner le boss ---
    public bool IsFullyStopped => currentForwardSpeed <= 0.05f;

    void Start()
    {
        currentForwardSpeed = forwardSpeed;
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }

    void Update()
    {
        if (isStoppedForBoss)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, brakingSpeed * Time.deltaTime);
        }
        else
        {
            currentForwardSpeed = forwardSpeed;
        }

        // On ne bouge que si la vitesse est supérieure à 0
        if (currentForwardSpeed > 0f)
        {
            transform.Translate(Vector3.forward * currentForwardSpeed * Time.deltaTime);
        }

        Vector3 newPosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, laneChangeSpeed * Time.deltaTime);

        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping)
            {
                Vector2 touchDeltaPosition = touch.deltaPosition;

                if (touchDeltaPosition.x > 50)
                {
                    if (currentLane < 2)
                    {
                        currentLane++;
                        ChangeLane();
                    }
                    isSwiping = false;
                }
                else if (touchDeltaPosition.x < -50)
                {
                    if (currentLane > 0)
                    {
                        currentLane--;
                        ChangeLane();
                    }
                    isSwiping = false;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isSwiping = false;
            }
        }
    }

    void ChangeLane()
    {
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
    }
}