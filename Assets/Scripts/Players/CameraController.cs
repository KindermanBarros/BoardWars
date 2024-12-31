using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    void Start()
    {

        offset = transform.position - player.position;
    }

    void LateUpdate()
    {

        transform.position = player.position + offset;
    }
}