using System;

public static partial class GameEvents
{
    public static Action<bool> OnCrosshairVisibilityChanged = delegate { }; // Cursor on/off.
    public static Action<string, float> OnShowHint = delegate { }; // (Gösterilecek Metin, Ekranda Kalma Süresi)
}