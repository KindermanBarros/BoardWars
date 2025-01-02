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
                Movement = 4;
                Power = 10;
                Health = 50;
                break;
            case CharacterVariant.StrongAttack:
                Movement = 3;
                Power = 30;
                Health = 35;
                break;
            case CharacterVariant.HighHealth:
                Movement = 2;
                Power = 10;
                Health = 75;
                break;
            case CharacterVariant.Default:
            default:
                Movement = 3;
                Power = 10;
                Health = 50;
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