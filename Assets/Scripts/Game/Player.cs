using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public string Name { get; private set; }

    public Player(string name)
    {
        Name = name;
    }
}
