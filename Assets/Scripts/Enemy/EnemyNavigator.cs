using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    [Header("Target Settings")]
    private Transform target;

    [Header("Attack Settings")]
    [Tooltip("Bebeðe ne kadar yaklaþýnca vurmaya baþlasýn?")]
    [SerializeField] private float attackRange = 1.5f;
    [Tooltip("Ýki saldýrý arasýnda ne kadar beklesin?")]
    [SerializeField] private float attackCooldown = 2.0f;
    [Tooltip("Saldýrý animasyonu ne kadar sürüyor? (O sýrada yürümez)")]
    [SerializeField] private float attackAnimDuration = 1.0f;

    private NavMeshAgent agent;
    private Enemy enemyScript;
    private float lastAttackTime;
    private bool isAttacking = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyScript = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (GameEvents.GetBabyTransform != null)
        {
            target = GameEvents.GetBabyTransform();
        }
    }

    // Enemy.cs tarafýndan EnemyData içindeki hýz ile çaðrýlýr
    public void SetSpeed(float speed)
    {
        if (agent != null) agent.speed = speed;
    }

    private void Start()
    {
        agent.acceleration = 12f;
        // Düþmanýn durma mesafesini saldýrý mesafesine eþitle
        agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        // Eðer düþman öldüyse veya þu an vurma animasyonundaysa düþünmeyi býrak
        if (enemyScript != null && enemyScript.IsDead) return;
        if (isAttacking) return;

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // Hedefe yeterince yakýnsa ve saldýrý süresi dolduysa
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
            // Hedef uzaktaysa ona doðru yürümeye devam et
            else if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true; // Vururken yürümeyi durdur
        lastAttackTime = Time.time;

        // Vurmadan önce yüzünü tam bebeðe dön
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        // Animator'deki 'attack' okunu ateþle
        enemyScript.TriggerAttackAnimation();

        // NOT: Ýleride bebeðin caný azalsýn istersen tam buraya hasar kodunu ekleyebilirsin.
        // Örn: GameEvents.OnBabyTakeDamage(enemyScript.data.damage);

        // Vurma animasyonunun bitmesini bekle
        yield return new WaitForSeconds(attackAnimDuration);

        // Eðer bu sýrada Asena onu vurup öldürmediyse yürümeye/saldýrmaya devam et
        if (!enemyScript.IsDead)
        {
            isAttacking = false;
            agent.isStopped = false;
        }
    }
}