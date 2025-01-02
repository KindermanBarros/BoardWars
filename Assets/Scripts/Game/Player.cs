using UnityEngine;
using System.Collections.Generic;

public enum CharacterVariant
{
    Default,
    FastMovement,
    StrongAttack,
    HighHealth
}

public class Player
{
    public string Name { get; private set; }
    public int Wins { get; private set; }
    public CharacterVariant Variant { get; private set; }
    public int Movement { get; private set; }
    public int Power { get; private set; }
    public int Health { get; private set; }

    public Player(string name, CharacterVariant variant)
    {
        Name = name;
        Variant = variant;
        ApplyVariant();
    }

    public Player(string name) : this(name, CharacterVariant.Default)
    {
    }

    private void ApplyVariant()
    {
        switch (Variant)
        {
            case CharacterVariant.FastMovement:
                Movement = 4;    // 4 moves per turn
                Power = 8;      // -2 power
                Health = 50;    // normal health
                break;
            case CharacterVariant.StrongAttack:
                Movement = 2;    // 2 moves per turn
                Power = 15;     // +5 power
                Health = 35;    // -15 health
                break;
            case CharacterVariant.HighHealth:
                Movement = 2;    // 2 moves per turn
                Power = 10;     // normal power
                Health = 75;    // +25 health
                break;
            case CharacterVariant.Default:
            default:
                Movement = 3;    // 3 moves per turn (default)
                Power = 10;     // normal power
                Health = 50;    // normal health
                break;
        }
    }

    public void AddWin()
    {
        Wins++;
        Debug.Log($"{Name} total wins: {Wins}");
    }

    public void ResetWins()
    {
        Wins = 0;
    }
}
