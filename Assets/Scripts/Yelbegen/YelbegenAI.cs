using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class YelbegenAI : MonoBehaviour, IDamageable
{
    private enum YelbegenState { ChasingBaby, Punching, TakingBaby, FleeingWithBaby, FleeingEmpty }

    [Header("AI Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float takeBabyDistance = 2.0f;
    [SerializeField] private float punchInterval = 2.0f;

    [Header("Animation Durations")]
    [SerializeField] private float punchAnimDuration = 1.5f;
    [SerializeField] private float takeBabyAnimDuration = 2.5f;

    [Header("References")]
    [SerializeField] private Transform handMountPoint;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform babyTransform;
    private Transform forestEscapePoint;

    private YelbegenState currentState = YelbegenState.ChasingBaby;
    private float punchTimer = 0f;
    private int damageHits = 0;
    private bool hasBaby = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        agent.speed = walkSpeed;
    }

    private void Start()
    {
        if (GameEvents.GetBabyTransform != null)
        {
            babyTransform = GameEvents.GetBabyTransform();
        }
    }

    public void SetForestPoint(Transform point) => forestEscapePoint = point;

    private void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        switch (currentState)
        {
            case YelbegenState.ChasingBaby:
                HandleChasing();
                break;
            case YelbegenState.FleeingWithBaby:
            case YelbegenState.FleeingEmpty:
                HandleFleeing();
                break;
        }
    }

    private void HandleChasing()
    {
        if (babyTransform == null) return;

        agent.SetDestination(babyTransform.position);

        if (Vector3.Distance(transform.position, babyTransform.position) <= takeBabyDistance)
        {
            StartCoroutine(TakeBabyRoutine());
            return;
        }

        punchTimer += Time.deltaTime;
        if (punchTimer >= punchInterval)
        {
            StartCoroutine(PunchRoutine());
        }
    }

    private IEnumerator PunchRoutine()
    {
        currentState = YelbegenState.Punching;
        agent.isStopped = true;
        punchTimer = 0f;

        anim.SetTrigger("Punch");
        GameEvents.OnPlayOneShotSFX("YelbegenPunchGround");

        yield return new WaitForSeconds(punchAnimDuration);

        if (currentState != YelbegenState.FleeingEmpty)
        {
            agent.isStopped = false;
            currentState = YelbegenState.ChasingBaby;
        }
    }

    private IEnumerator TakeBabyRoutine()
    {
        currentState = YelbegenState.TakingBaby;
        agent.isStopped = true;

        transform.LookAt(new Vector3(babyTransform.position.x, transform.position.y, babyTransform.position.z));

        anim.SetTrigger("TakeBaby");
        GameEvents.OnPlayOneShotSFX("YelbegenLaugh");

        yield return new WaitForSeconds(takeBabyAnimDuration);

        if (babyTransform != null)
        {
            GameEvents.OnBabyStolen(handMountPoint);
        }

        hasBaby = true;
        anim.SetBool("HasBaby", true);

        currentState = YelbegenState.FleeingWithBaby;
        agent.isStopped = false;

        GameEvents.OnShowHint("Yelbegen bebeði aldý! Kaçmadan onu durdur!", 4f);
    }

    private void HandleFleeing()
    {
        if (forestEscapePoint != null)
        {
            agent.SetDestination(forestEscapePoint.position);

            if (!agent.pathPending && agent.remainingDistance <= 2f)
            {
                if (hasBaby)
                {
                    Debug.Log("GAME OVER: Yelbegen bebeði ormana kaçýrdý!");
                }
                Destroy(gameObject);
            }
        }
    }

    public void TakeDamage(float amount, EntityTeam attackerTeam)
    {
        if (currentState == YelbegenState.FleeingEmpty) return;

        if (attackerTeam == EntityTeam.Player)
        {
            damageHits++;
            GameEvents.OnPlayOneShotSFX("YelbegenHurt");

            if (damageHits >= 3)
            {
                StartFleeing();
            }
        }
    }

    private void StartFleeing()
    {
        if (hasBaby && babyTransform != null)
        {
            hasBaby = false;
            anim.SetBool("HasBaby", false);

            // BabyManager zaten kendi Y (Yükseklik) ayarýný otomatik düzeltir, sadece X ve Z'yi yolluyoruz
            GameEvents.OnTryDropRequested(babyTransform.position);
            GameEvents.OnShowHint("Yelbegen bebeði býraktý ve kaçýyor!", 3f);
        }

        StopAllCoroutines();

        currentState = YelbegenState.FleeingEmpty;
        agent.isStopped = false;

        GameEvents.OnPlayOneShotSFX("YelbegenFlee");
    }
}