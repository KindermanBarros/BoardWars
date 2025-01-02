using UnityEngine;
using System.Linq;

public class Battle
{
    private const int DICE_SIDES = 20;
    private const int BASE_DICE_COUNT = 3;
    private const int RING_OUT_DAMAGE = 999;

    public class BattleResult
    {
        public PlayerPiece Winner { get; set; }
        public PlayerPiece Loser { get; set; }
        public int[] AttackerRolls { get; set; }
        public int[] DefenderRolls { get; set; }
        public int DamageDealt { get; set; }
    }

    public static BattleResult ResolveBattle(PlayerPiece attacker, PlayerPiece defender, bool isRingOut = false)
    {
        Debug.Log($"=== BATTLE START: {attacker.name} vs {defender.name} ===");

        if (isRingOut)
        {
            return new BattleResult
            {
                Winner = attacker,
                Loser = defender,
                AttackerRolls = new int[] { 20 },
                DefenderRolls = new int[] { 0 },
                DamageDealt = RING_OUT_DAMAGE
            };
        }

        PlayerPiece winner;
        PlayerPiece loser;
        int[] attackerRolls;
        int[] defenderRolls;

        int attackerDiceCount = BASE_DICE_COUNT + attacker.GetExtraDiceRolls();
        int defenderDiceCount = BASE_DICE_COUNT + defender.GetExtraDiceRolls();

        attackerRolls = RollDice(attackerDiceCount);
        defenderRolls = RollDice(defenderDiceCount);

        Debug.Log($"{attacker.name}'s rolls ({attackerDiceCount} dice): {string.Join(", ", attackerRolls)}");
        Debug.Log($"{defender.name}'s rolls ({defenderDiceCount} dice): {string.Join(", ", defenderRolls)}");

        int attackerWins = 0;
        int defenderWins = 0;
        int comparisons = Mathf.Min(attackerDiceCount, defenderDiceCount);

        Debug.Log("\nComparing dice pairs:");
        for (int i = 0; i < comparisons; i++)
        {
            bool attackerWinsRoll = attackerRolls[i] >= defenderRolls[i];
            if (attackerWinsRoll) attackerWins++;
            else defenderWins++;

            Debug.Log($"Pair {i + 1}: {attackerRolls[i]} vs {defenderRolls[i]} - {(attackerWinsRoll ? attacker.name : defender.name)} wins this roll");
        }

        winner = attackerWins > defenderWins ? attacker : defender;
        loser = attackerWins > defenderWins ? defender : attacker;

        int damageDealt = winner.GetTotalAttack();

        Debug.Log($"\nBattle Result: {winner.name} wins" + (isRingOut ? " by ring-out!" : ""));
        Debug.Log($"Winner's Power: {damageDealt}");
        Debug.Log($"Damage to be dealt to {loser.name}: {damageDealt}");
        Debug.Log("=== BATTLE END ===\n");

        return new BattleResult
        {
            Winner = winner,
            Loser = loser,
            AttackerRolls = attackerRolls,
            DefenderRolls = defenderRolls,
            DamageDealt = damageDealt
        };
    }

    private static int[] RollDice(int count)
    {
        int[] rolls = new int[count];
        for (int i = 0; i < count; i++)
        {
            rolls[i] = Random.Range(1, DICE_SIDES + 1);
        }
        return rolls;
    }
}
