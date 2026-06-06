using System;
using UnityEngine;

// Oyundaki taraflar. Bu sayede kimin kime hasar verebileceðini ayýrt edeceðiz.
public enum EntityTeam { Player, Enemy, Environment }

public static partial class GameEvents
{
    public static Action<float, float> OnPlayerHealthChanged = delegate { }; 
    public static Action OnPlayerDied = delegate { };

    public static Action<GameObject> OnEnemyDied = delegate { };
}