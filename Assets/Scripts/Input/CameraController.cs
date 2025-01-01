using UnityEngine;

public class CameraController : MonoBehaviour, IInputHandler
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 10f, -5f); // Camera offset from target

    private Camera cam;
    private Vector3 velocity = Vector3.zero;
    private Transform target;
    private Vector3 targetPosition;

    private void Start()
    {
        cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
        }
        InputManager.Instance.RegisterHandler(this);
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
            // Calculate target position with offset
            Vector3 desiredPosition = target.position + offset;

            // Smoothly move camera
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
    }

    public void HandleInput()
    {
        // Manual camera control only when not following a target
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

        transform.Translate(direction * panSpeed * Time.deltaTime, Space.World);
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

