using UnityEngine;

public class CameraPathController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera controlledCamera; // Камера, которой будем управлять
    public Transform cameraTarget; // Цель, на которую должна смотреть камера

    [Header("Path Settings")]
    public Transform[] pathPoints;  // Точки пути
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public bool loop = true;
    public bool autoStart = true;

    [Header("Debug")]
    [SerializeField] private int currentPointIndex = 0;
    [SerializeField] private bool isMovingForward = true;
    private bool isActive = false;

    void Start()
    {
        if (controlledCamera == null)
        {
            controlledCamera = Camera.main;
            Debug.LogWarning("Camera not assigned, using Main Camera");
        }

        if (autoStart) StartMovement();
    }

    void Update()
    {
        if (!isActive || pathPoints.Length == 0 || controlledCamera == null) return;

        MoveCameraAlongPath();
        RotateCameraToTarget();
        CheckWaypointReached();
    }

    private void MoveCameraAlongPath()
    {
        Vector3 targetPosition = pathPoints[currentPointIndex].position;
        controlledCamera.transform.position = Vector3.MoveTowards(
            controlledCamera.transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );
    }

    private void RotateCameraToTarget()
    {
        if (cameraTarget == null) return;

        Vector3 direction = cameraTarget.position - controlledCamera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        controlledCamera.transform.rotation = Quaternion.Slerp(
            controlledCamera.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void CheckWaypointReached()
    {
        float distance = Vector3.Distance(
            controlledCamera.transform.position,
            pathPoints[currentPointIndex].position
        );

        if (distance < 0.1f)
        {
            UpdateWaypointIndex();
        }
    }

    private void UpdateWaypointIndex()
    {
        if (isMovingForward)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Length)
            {
                if (loop)
                {
                    currentPointIndex = pathPoints.Length - 2;
                    isMovingForward = false;
                }
                else
                {
                    currentPointIndex = pathPoints.Length - 1;
                    PauseMovement();
                }
            }
        }
        else
        {
            currentPointIndex--;
            if (currentPointIndex < 0)
            {
                if (loop)
                {
                    currentPointIndex = 1;
                    isMovingForward = true;
                }
                else
                {
                    currentPointIndex = 0;
                    PauseMovement();
                }
            }
        }
    }

    public void StartMovement()
    {
        isActive = true;
    }

    public void PauseMovement()
    {
        isActive = false;
    }

    public void ResumeMovement()
    {
        isActive = true;
    }

    public void StopMovement()
    {
        isActive = false;
        currentPointIndex = 0;
        isMovingForward = true;
        if (controlledCamera != null && pathPoints.Length > 0)
        {
            controlledCamera.transform.position = pathPoints[0].position;
        }
    }

    public void ToggleMovement()
    {
        isActive = !isActive;
    }

    public void SetTargetPoint(Transform newTarget)
    {
        cameraTarget = newTarget;
    }

    public void AddPathPoint(Transform newPoint)
    {
        // Создаем новый массив с дополнительной точкой
        Transform[] newArray = new Transform[pathPoints.Length + 1];
        pathPoints.CopyTo(newArray, 0);
        newArray[pathPoints.Length] = newPoint;
        pathPoints = newArray;
    }
}