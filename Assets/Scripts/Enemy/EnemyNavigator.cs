using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;

    private NavMeshAgent agent;

    private void OnEnable()
    {
        target = GameEvents.GetBabyTransform.Invoke();
        Debug.Log("EnemyNavigator: Target set to " + target.name);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.speed = 3.5f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 0.5f;
    }

    private void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            Debug.Log("EnemyNavigator: Moving towards target at " + target.position);
        }
    }
}
