using UnityEngine;

public class PlayerPiece : MonoBehaviour
{
    [field: SerializeField] public int PlayerNumber { get; private set; }
    public int healthPoints = 100;
    public int attackPoints = 20;

    public void TakeDamage(int damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0)
        {
            Die();
        }
    }

    public void Attack(PlayerPiece target)
    {
        target?.TakeDamage(attackPoints);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}