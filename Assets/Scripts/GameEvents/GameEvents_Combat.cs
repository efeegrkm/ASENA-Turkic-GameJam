
using System;

public static partial class GameEvents
{
    public static Action OnBowDrawStarted = delegate { }; 
    public static Action OnBowDrawCanceled = delegate { }; 
    public static Action<float> OnBowShooted = delegate { }; 
    public static Action<int> OnArrowCountChanged = delegate { }; //UI update.
    public static Action<bool> OnAimStateChanged = delegate { }; // true: Niþan alýyor false: býraktý.
}