using System;
public static partial class GameEvents
{
    // true: Kurda dönüþüyor, false: Ýnsana dönüþüyor
    public static Action<bool> OnFormChangeStarted = delegate { };
    public static Action<bool> OnFormChanged = delegate { };
    public static Action OnHumanDodgeStarted = delegate { };
    public static Action OnHumanDodgeEnded = delegate { };
}