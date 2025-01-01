using UnityEngine;
using System.Collections;

public class PlayerPiece : MonoBehaviour
{
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public HexCell CurrentCell { get; private set; }
    public PlayerType Type { get; private set; }
    public Player Player { get; private set; }

    public enum PlayerType
    {
        Player1,
        Player2
    }

    private const float PIECE_HEIGHT = 0.5f; // Height constant

    private void Awake()
    {
        // Add collider for mouse interaction
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.8f, 1f, 0.8f); // Smaller than hex cell
        collider.center = new Vector3(0, 0.5f, 0);
        collider.isTrigger = false; // Non-trigger for proper clicking

        // Create visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.up * 0.5f;
        visual.transform.localScale = Vector3.one * 0.8f;
        Destroy(visual.GetComponent<Collider>()); // Remove sphere collider
    }

    public void Initialize(Player player, int health, int attack, HexCell startCell, PlayerType type)
    {
        Player = player;
        Health = health;
        Attack = attack;
        CurrentCell = startCell;
        Type = type;
        transform.position = startCell.transform.position;

        // Set color based on player type
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        Material newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = type == PlayerType.Player1 ? Color.red : Color.blue;
        renderer.material = newMaterial;
    }

    public void MoveTo(HexCell targetCell)
    {
        CurrentCell = targetCell;
        // Position piece higher than the hex cell
        transform.position = targetCell.transform.position + Vector3.up * PIECE_HEIGHT;
        StartCoroutine(JumpAnimation());
    }

    public void ResetPosition(HexCell cell)
    {
        transform.position = cell.transform.position + Vector3.up * PIECE_HEIGHT;
        CurrentCell = cell;
    }

    private IEnumerator JumpAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 midPos = startPos + Vector3.up * 0.5f;
        float duration = 0.2f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            transform.position = Vector3.Lerp(startPos, midPos, normalizedTime);
            yield return null;
        }

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            transform.position = Vector3.Lerp(midPos, startPos, normalizedTime);
            yield return null;
        }
    }
}