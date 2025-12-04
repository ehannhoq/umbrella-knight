using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] ScriptableObject idleBehavior;

    private ScriptableObject currentBehavior;

    public float minimumMoveDistance;
    public float minimumAttackDistance;
    
    [HideInInspector] public GameObject player;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public float distanceToPlayer;

    void Start()
    {
        currentBehavior = idleBehavior;
        StartBehavior(currentBehavior);

        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

        agent.angularSpeed = 500;
    }

    void Update()
    {
        distanceToPlayer = (transform.position - player.transform.position).magnitude;

        ExecuteBehavior(currentBehavior);
    }

    void StartBehavior(ScriptableObject behavior)
    {
        if (behavior is IEnemyBehavior enemyBehavior)
        {
            enemyBehavior.Initialize(this);
        }
    }

    void ExecuteBehavior(ScriptableObject behavior)
    {
        if (behavior is IEnemyBehavior enemyBehavior)
        {
            enemyBehavior.Execute(this);
        }
    }

    public void ChangeCurrentBehavior(ScriptableObject newBehavior)
    {
        agent.SetDestination(transform.position);
        currentBehavior = newBehavior;
    }

    public void MoveToPosition(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}
