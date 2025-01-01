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

    private const float PIECE_HEIGHT = 0.5f;
    private const int MAX_HEALTH = 100;
    private int temporaryAttackBonus;

    private void Awake()
    {

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.8f, 1f, 0.8f);
        collider.center = new Vector3(0, 0.5f, 0);
        collider.isTrigger = false;

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.up * 0.5f;
        visual.transform.localScale = Vector3.one * 0.8f;
        Destroy(visual.GetComponent<Collider>());
    }

    public void Initialize(Player player, int health, int attack, HexCell startCell, PlayerType type)
    {
        Player = player;
        Health = health;
        Attack = attack;
        CurrentCell = startCell;
        Type = type;
        transform.position = startCell.transform.position;
    }

    public void MoveTo(HexCell targetCell)
    {
        CurrentCell = targetCell;
        transform.position = targetCell.transform.position + Vector3.up * PIECE_HEIGHT;
        StartCoroutine(JumpAnimation());
    }

    public void ResetPosition(HexCell cell)
    {
        transform.position = cell.transform.position + Vector3.up * PIECE_HEIGHT;
        CurrentCell = cell;
    }

    public void AddTemporaryAttack(int amount)
    {
        temporaryAttackBonus += amount;
        Debug.Log($"Added {amount} temporary attack. Total attack now: {GetTotalAttack()}");
    }

    public void Heal(int amount)
    {
        int newHealth = Mathf.Min(Health + amount, MAX_HEALTH);
        int actualHeal = newHealth - Health;
        Health = newHealth;
        Debug.Log($"Healed for {actualHeal}. Current health: {Health}/{MAX_HEALTH}");
    }

    public void OnTurnEnd()
    {
        if (temporaryAttackBonus > 0)
        {
            Debug.Log($"Removing temporary attack bonus: {temporaryAttackBonus}");
            temporaryAttackBonus = 0;
        }
    }

    private void EndTurn()
    {
        temporaryAttackBonus = 0;
    }

    public int GetTotalAttack() => Attack + temporaryAttackBonus;

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