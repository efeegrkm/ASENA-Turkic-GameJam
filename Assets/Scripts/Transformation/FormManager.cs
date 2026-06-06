using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FormManager : MonoBehaviour
{
    [Header("Transformation Settings")]
    [SerializeField] private float transformDuration = 2.0f;
    [SerializeField] private InputActionReference transformAction;

    [Header("Models")]
    [SerializeField] private GameObject humanModelObject;
    [SerializeField] private GameObject wolfModelObject;

    private bool isWolf = false;
    private bool isTransforming = false;
    private BabyState currentBabyState = BabyState.Dropped;

    private void Start()
    {
        humanModelObject.SetActive(true);
        wolfModelObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnBabyStateChanged += UpdateBabyState;
        if (transformAction != null) transformAction.action.Enable();
        transformAction.action.performed += TryTransform;
    }

    private void OnDisable()
    {
        GameEvents.OnBabyStateChanged -= UpdateBabyState;
        if (transformAction != null) transformAction.action.Disable();
        transformAction.action.performed -= TryTransform;
    }

    private void UpdateBabyState(BabyState state) => currentBabyState = state;

    private void TryTransform(InputAction.CallbackContext context)
    {
        if (isTransforming) return;

        // KURAL 1: Sýrtta bebek varken kurda dönüþülemez
        if (!isWolf && currentBabyState == BabyState.CarriedOnBack)
        {
            GameEvents.OnShowHint("Sýrtýnda Oðuz varken Asena form deðiþtiremez! Önce onu yere býrak.", 4f);
            return;
        }

        // KURAL 2: Kurt aðzýnda puseti tutarken insana dönüþemez (HATA 1 ÇÖZÜMÜ)
        if (isWolf && currentBabyState == BabyState.CarriedInMouth)
        {
            GameEvents.OnShowHint("Aðzýnda puset varken insana dönüþemezsin! Önce G tuþunu býrak.", 4f);
            return;
        }

        StartCoroutine(TransformRoutine());
    }

    private IEnumerator TransformRoutine()
    {
        isTransforming = true;
        bool targetFormIsWolf = !isWolf;

        GameEvents.OnFormChangeStarted(targetFormIsWolf);
        GameEvents.OnPlayOneShotSFX("TransformHowl");

        yield return new WaitForSeconds(transformDuration);

        isWolf = targetFormIsWolf;
        humanModelObject.SetActive(!isWolf);
        wolfModelObject.SetActive(isWolf);

        GameEvents.OnFormChanged(isWolf);
        isTransforming = false;
    }
}