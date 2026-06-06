using System;
using UnityEngine;

public static partial class GameEvents
{
    public static Action<float> OnIncreaseFOVTo = delegate { };
    public static Action OnResetFOV = delegate { };
    public static Action OnStartDefaultOrbit = delegate { };
    public static Action<GameObject, float, float, float> OnStartCustomOrbit = delegate { };
    public static Action<bool> OnSetCursorState = delegate { };
    public static Action<bool> OnToggleAimCamera = delegate { };
}