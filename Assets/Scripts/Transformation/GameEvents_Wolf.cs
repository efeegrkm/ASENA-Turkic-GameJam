using System;
using UnityEngine;

public static partial class GameEvents
{
    public static Action OnWolfMeleeStarted = delegate { };
    public static Action OnWolfMeleeCompleted = delegate { };

    public static Action OnWolfDashStarted = delegate { };
    public static Action OnWolfDashCompleted = delegate { };

    public static Action<bool> OnWolfDragStateChanged = delegate { }; // true: Sürüklemeye baþladý
}