using UnityEngine;

public class CameraController : MonoBehaviour, IInputHandler
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 32f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 48f, -12f);
    [SerializeField] private float tiltAngle = 60f;
    [SerializeField] private float nearClipPlane = 0.1f;
    [SerializeField] private float farClipPlane = 1000f;
    [SerializeField] private float boundaryBuffer = 80f;
    [SerializeField] private float defaultZoom = 16f;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Transform target;
    private Vector3 targetPosition;
    private Vector3 gridMin;
    private Vector3 gridMax;

    private void Start()
    {
        cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = defaultZoom;
            cam.nearClipPlane = nearClipPlane;
            cam.farClipPlane = farClipPlane;
            transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);

            if (FindObjectOfType<HexGrid>() is HexGrid grid)
            {
                Vector3 gridCenter = grid.GetGridCenter();
                CalculateGridBoundaries(grid);
                transform.position = gridCenter + new Vector3(0, defaultZoom, -defaultZoom * 0.5f);
            }
        }
        InputManager.Instance.RegisterHandler(this);
    }

    private void CalculateGridBoundaries(HexGrid grid)
    {
        float gridWidth = grid.Width * grid.HexSize * 2f;
        float gridHeight = grid.Height * Mathf.Sqrt(3f) * grid.HexSize;

        gridMin = grid.transform.position - new Vector3(boundaryBuffer, 0, boundaryBuffer * 2f);
        gridMax = grid.transform.position + new Vector3(gridWidth + boundaryBuffer, 0, gridHeight + boundaryBuffer);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetPosition = target.position + offset;
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
            transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
        }
    }

    public void HandleInput()
    {
        if (target == null)
        {
            HandleMovement();
        }
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            direction += Vector3.right;
        }

        Vector3 newPosition = transform.position + direction * panSpeed * Time.deltaTime;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        newPosition.x = Mathf.Clamp(newPosition.x, gridMin.x + halfWidth, gridMax.x - halfWidth);
        newPosition.z = Mathf.Clamp(newPosition.z, gridMin.z + halfHeight * 1.5f, gridMax.z - halfHeight);

        transform.position = newPosition;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void OnDestroy()
    {
        InputManager.Instance.UnregisterHandler(this);
    }

    public Camera GetCamera() => cam;
}

