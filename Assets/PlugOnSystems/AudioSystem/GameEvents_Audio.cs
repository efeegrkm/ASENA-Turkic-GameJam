using System;
public static partial class GameEvents
{
    public static Action<string> OnPlayMusic = delegate { };
    public static Action OnStopMusic = delegate { };
    public static Action OnStopLoopedSFX = delegate { };
    public static Action<string> OnPlayOneShotSFX = delegate { };
    public static Action<string> OnPlayLoopedSFX = delegate { };
    public static Action<float> OnSkipMusicTime = delegate { };
}