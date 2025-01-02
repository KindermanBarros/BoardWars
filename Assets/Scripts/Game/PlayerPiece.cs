using UnityEngine;
using System.Collections;
using System.Linq;

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
    private int MAX_HEALTH;
    private int attackBonus;
    private int rerollCount;

    [SerializeField] private GameObject damageParticlePrefab;

    private void Awake()
    {

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.8f, 1f, 0.8f);
        collider.center = new Vector3(0, 0.5f, 0);
        collider.isTrigger = false;
    }

    private void SetMaxHealth()
    {
        MAX_HEALTH = Player.Health;
    }

    public void Initialize(Player player, HexCell startCell, PlayerType type)
    {
        Player = player;
        SetMaxHealth();
        Health = MAX_HEALTH;
        Attack = player.Power;
        attackBonus = 0;
        rerollCount = 0;

        CurrentCell = startCell;
        transform.position = startCell.transform.position + Vector3.up * PIECE_HEIGHT;
        Type = type;

        Debug.Log($"Initialized {type} with variant {player.Variant}: Power={Attack}, Health={Health}/{MAX_HEALTH}, Moves={player.Movement}");
    }

    public void MoveTo(HexCell targetCell)
    {
        if (targetCell == null) return;

        CurrentCell = targetCell;
        transform.position = targetCell.transform.position + Vector3.up * PIECE_HEIGHT;
        StartCoroutine(JumpAnimation());

        if (!BoardGame.Instance.isAttacking)
        {
            AudioManager.Instance?.PlayMove();
        }
    }

    public void ResetPosition(HexCell cell)
    {
        transform.position = cell.transform.position + Vector3.up * PIECE_HEIGHT;
        CurrentCell = cell;
    }

    public void AddTemporaryAttack(int amount)
    {
        attackBonus += amount;
        Debug.Log($"Added {amount} attack. Total attack now: {GetTotalAttack()}");
    }

    public void Heal(int amount)
    {
        if (MAX_HEALTH <= 0) SetMaxHealth();
        int newHealth = Mathf.Min(Health + amount, MAX_HEALTH);
        int actualHeal = newHealth - Health;
        Health = newHealth;
        Debug.Log($"Healed for {actualHeal}. Current health: {Health}/{MAX_HEALTH}");
    }

    public void TakeDamage(int damage, PlayerPiece attacker)
    {
        Health = Mathf.Max(0, Health - damage);
        Debug.Log($"{gameObject.name} takes {damage} damage. Health: {Health}");

        if (damageParticlePrefab != null && attacker != null)
        {
            Vector3 directionToAttacker = (attacker.transform.position - transform.position).normalized;
            Vector3 particlePosition = transform.position + directionToAttacker * 0.5f + Vector3.up * 0.5f;
            GameObject particleObj = Instantiate(damageParticlePrefab, particlePosition, Quaternion.LookRotation(-directionToAttacker));

            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constant;
                Destroy(particleObj, totalDuration);
            }
            else
            {
                Destroy(particleObj, 2f);
            }
        }

        if (Health <= 0)
        {
            Debug.Log($"{gameObject.name} has been defeated by HP loss!");
            PlayerPiece winner = BoardGame.Instance.players.First(p => p != this);
            BoardGame.Instance.HandleWin(winner);
            return;
        }
    }

    public void OnTurnEnd()
    {
        attackBonus = 0;
        rerollCount = 0;
        Debug.Log($"{gameObject.name} turn ended, reset temporary bonuses");
    }

    public int GetTotalAttack() => Attack + attackBonus;

    public int GetTemporaryAttack()
    {
        return attackBonus;
    }

    public void AddExtraDiceRoll(int amount)
    {
        rerollCount += amount;
        Debug.Log($"Added {amount} rerolls. Total rerolls now: {rerollCount}");
        BoardGame.Instance.UpdateUI();
    }

    public int GetExtraDiceRolls()
    {
        return rerollCount;
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

    public void PlayResetAnimation()
    {
        StartCoroutine(ResetAnimation());
    }

    private IEnumerator ResetAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = CurrentCell.transform.position + Vector3.up * PIECE_HEIGHT;

        Vector3 highPoint = startPos + Vector3.up * 2f;
        float duration = 0.3f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            transform.position = Vector3.Lerp(startPos, highPoint, normalizedTime);
            yield return null;
        }

        duration = 0.2f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            transform.position = Vector3.Lerp(highPoint, targetPos, normalizedTime);
            yield return null;
        }

        transform.position = targetPos;
    }
}