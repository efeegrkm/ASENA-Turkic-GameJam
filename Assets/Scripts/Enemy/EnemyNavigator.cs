using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    [Header("Target Settings")]
    private Transform target;

    [Header("Attack Settings")]
    [Tooltip("Bebe�e ne kadar yakla��nca vurmaya ba�las�n?")]
    [SerializeField] private float attackRange = 1.5f;
    [Tooltip("�ki sald�r� aras�nda ne kadar beklesin?")]
    [SerializeField] private float attackCooldown = 2.0f;
    [Tooltip("Sald�r� animasyonu ne kadar s�r�yor? (O s�rada y�r�mez)")]
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
        GameEvents.OnPlayerGotAttacked += HandlePlayerGotAttacked;
        if (GameEvents.GetBabyTransform != null)
        {
            target = GameEvents.GetPlayerTransform();
        }
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerGotAttacked -= HandlePlayerGotAttacked;
    }

    // Enemy.cs taraf�ndan EnemyData i�indeki h�z ile �a�r�l�r
    public void SetSpeed(float speed)
    {
        if (agent != null) agent.speed = speed;
    }

    private void Start()
    {
        agent.acceleration = 12f;
        // D��man�n durma mesafesini sald�r� mesafesine e�itle
        agent.stoppingDistance = attackRange;
    }

    private void Update()
    {
        // E�er d��man �ld�yse veya �u an vurma animasyonundaysa d���nmeyi b�rak
        if (enemyScript != null && enemyScript.IsDead) return;
        if (isAttacking) return;

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            // Hedefe yeterince yak�nsa ve sald�r� s�resi dolduysa
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                GameEvents.OnPlayerGotAttacked();
                StartCoroutine(AttackRoutine());
            }
            // Hedef uzaktaysa ona do�ru y�r�meye devam et
            else if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        Debug.Log($"{gameObject.name} is attacking!");
        isAttacking = true;
        agent.isStopped = true; // Vururken y�r�meyi durdur
        lastAttackTime = Time.time;

        // Vurmadan �nce y�z�n� tam bebe�e d�n
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        // Animator'deki 'attack' okunu ate�le
        enemyScript.TriggerAttackAnimation();

        // NOT: �leride bebe�in can� azals�n istersen tam buraya hasar kodunu ekleyebilirsin.
        // �rn: GameEvents.OnBabyTakeDamage(enemyScript.data.damage);

        // Vurma animasyonunun bitmesini bekle
        yield return new WaitForSeconds(attackAnimDuration);

        // E�er bu s�rada Asena onu vurup �ld�rmediyse y�r�meye/sald�rmaya devam et
        if (!enemyScript.IsDead)
        {
            isAttacking = false;
            agent.isStopped = false;
        }
    }

    private void HandlePlayerGotAttacked()
    {
        
    }
}