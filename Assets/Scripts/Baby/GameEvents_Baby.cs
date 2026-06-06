using System;
using UnityEngine;

public enum BabyState { Dropped, CarriedOnBack, CarriedInMouth }

public static partial class GameEvents
{
    public static Action<BabyState> OnBabyStateChanged = delegate { };
    public static Action<float, float> OnBabyHungerChanged = delegate { };
    public static Action<bool> OnBabyCrying = delegate { };

    public static Action<Vector3> OnTryNurseRequested = delegate { };
    public static Action<Transform> OnTryPickupRequested = delegate { }; // Sýrta almak için
    public static Action<Transform> OnTryDragRequested = delegate { };   // Aðýza almak (sürüklemek) için
    public static Action<Vector3> OnTryDropRequested = delegate { };

    public static Func<Transform> GetBabyTransform;

    public static Action OnBabyPickupStarted = delegate { };
    public static Action OnBabyDropStarted = delegate { };
    public static Action OnBabyNurseStarted = delegate { };
    public static Action OnBabyNurseCompleted = delegate { };
}