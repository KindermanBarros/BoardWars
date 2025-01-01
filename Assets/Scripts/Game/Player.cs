using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public string Name { get; private set; }
    public int Wins { get; private set; }

    public Player(string name)
    {
        Name = name;
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
