using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WolfAnimationController : MonoBehaviour
{
    private Animator anim;
    private Vector3 lastPosition;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Obje aktifleþtiði an (Kurda dönüþüldüðünde) son konumu sýfýrla ki saçma bir hýz patlamasý olmasýn
        lastPosition = transform.position;

        GameEvents.OnWolfMeleeStarted += PlayAttack;
        GameEvents.OnWolfDashStarted += PlayDash;
        GameEvents.OnWolfDragStateChanged += SetDragState;
        GameEvents.OnFormChangeStarted += PlayTransformToHuman;
    }

    private void OnDisable()
    {
        GameEvents.OnWolfMeleeStarted -= PlayAttack;
        GameEvents.OnWolfDashStarted -= PlayDash;
        GameEvents.OnWolfDragStateChanged -= SetDragState;
        GameEvents.OnFormChangeStarted -= PlayTransformToHuman;
    }

    private void Update()
    {
        // ÇÖZÜM: Fizik motoru (CharacterController) güncellemelerini beklemeden,
        // karakterin sahnede gerçekte ne kadar hareket ettiðini hesaplýyoruz.
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;

        // Sadece X ve Z eksenindeki yatay hýzý bul (aþaðý düþerken veya zýplarken yürüme animasyonuna girmemesi için)
        Vector3 horizontalVelocity = new Vector3(movement.x, 0f, movement.z) / Time.deltaTime;

        // Hesaplanan bu saf hýzý animatöre gönder
        anim.SetFloat("Speed", horizontalVelocity.magnitude);

        lastPosition = currentPosition;
    }

    // --- EVENT TETÝKLEYÝCÝLERÝ ---

    private void PlayAttack()
    {
        anim.SetTrigger("Attack");
    }

    private void PlayDash()
    {
        anim.SetTrigger("Dash");
    }

    private void SetDragState(bool isDragging)
    {
        anim.SetBool("IsDragging", isDragging);
    }

    private void PlayTransformToHuman(bool isTargetingWolf)
    {
        // Eðer hedeflenen form KURT DEÐÝLSE (yani insana dönüþüyorsak) tetikle
        if (!isTargetingWolf)
        {
            anim.SetTrigger("Transform");
        }
    }
}