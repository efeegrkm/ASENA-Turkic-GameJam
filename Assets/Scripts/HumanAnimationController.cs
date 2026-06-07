using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HumanAnimationController : MonoBehaviour
{
    private Animator anim;
    private Vector3 lastPosition;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Model aktifleþtiðinde son konumu kaydet (Hýzýn aniden fýrlamasýný önler)
        lastPosition = transform.position;

        // --- EVENT DÝNLEYÝCÝLERÝ ---
        GameEvents.OnAimStateChanged += SetAimState;
        GameEvents.OnBabyPickupStarted += PlayPickupBaby;
        GameEvents.OnBabyNurseStarted += PlayNurseBaby;
        GameEvents.OnFormChangeStarted += PlayTransformToWolf;
        GameEvents.OnHumanDodgeStarted += PlayDodge;
    }

    private void OnDisable()
    {
        // Hafýza sýzýntýsý olmamasý için dinlemeyi býrak
        GameEvents.OnAimStateChanged -= SetAimState;
        GameEvents.OnBabyPickupStarted -= PlayPickupBaby;
        GameEvents.OnBabyNurseStarted -= PlayNurseBaby;
        GameEvents.OnFormChangeStarted -= PlayTransformToWolf;
        GameEvents.OnHumanDodgeStarted -= PlayDodge;
    }

    private void Update()
    {
        // HIZ HESAPLAMASI (rig_idle ve rig_Walk geçiþleri için Speed parametresi)
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;

        // Zýplarken veya havadan düþerken karakterin yürüme animasyonuna girmemesi için Y eksenini yoksayýyoruz
        Vector3 horizontalVelocity = Vector3.zero;
        if (Time.deltaTime > 0)
        {
            horizontalVelocity = new Vector3(movement.x, 0f, movement.z) / Time.deltaTime;
        }

        anim.SetFloat("Speed", horizontalVelocity.magnitude);

        lastPosition = currentPosition;
    }

    // --- ANIMATOR TETÝKLEYÝCÝLERÝ ---

    private void SetAimState(bool isAiming)
    {
        // rig_yay_cikar, rig_yay_germe ve rig_yay_koy zincirini yönetir
        anim.SetBool("IsAiming", isAiming);
    }

    private void PlayPickupBaby()
    {
        // rig_bebeði_alma animasyonunu oynatýr
        anim.SetTrigger("PickupBaby");
    }

    private void PlayNurseBaby()
    {
        // rig_emzirme animasyonunu oynatýr
        anim.SetTrigger("Nurse");
    }

    private void PlayDodge()
    {
        // rig_dodge animasyonunu oynatýr
        anim.SetTrigger("Dodge");
    }

    private void PlayTransformToWolf(bool isTargetingWolf)
    {
        // Sadece Kurda dönüþüyorsak (T'ye bastýðýmýzda) insandaki rig_transform'u oynatýr
        if (isTargetingWolf)
        {
            anim.SetTrigger("Transform");
        }
    }
}