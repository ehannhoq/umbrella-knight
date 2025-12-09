using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Tooltip("Optional")]
    [SerializeField] ScriptableObject spawnBehavior;
    [SerializeField] ScriptableObject idleBehavior;
    [SerializeField] ScriptableObject stunnedBehavior;


    [Header("Enemy Parameters")]
    public float health;
    public float maxHealth;
    public float movementSpeed;
    public bool takesKnockback;
    public bool getsStunned;
    public float stunDuration;
    public float immunityTime = 0.55f;
    public bool immune;
    public bool dead;
    public bool canSeePlayer;


    [Header("AI Parameters")]
    public string currentBehaviorName;
    public float minimumMoveDistance;
    public float minimumAttackDistance;
    public Dictionary<string, object> data;

    [HideInInspector] public GameObject player;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public float distanceToPlayer;

    private ScriptableObject currentBehavior;
    public bool useRigidBody;

    void Start()
    {
        data = new Dictionary<string, object>();
        health = maxHealth;

        currentBehavior = idleBehavior;
        InitializeBehavior(currentBehavior);
        InitializeBehavior(spawnBehavior);

        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();


        rb.freezeRotation = true;
        agent.speed = movementSpeed;
        agent.acceleration = movementSpeed;

        agent.enabled = false;
        useRigidBody = false;
    }

    void Update()
    {
        if (dead) return;

        distanceToPlayer = (transform.position - player.transform.position).magnitude;
        canSeePlayer = !Physics.Linecast(transform.position + Vector3.up * (agent.height / 2f), player.transform.position + Vector3.up * 0.8f, ~(LayerMask.GetMask("Ignore Collision") | LayerMask.GetMask("NPC") | LayerMask.GetMask("NPCCollider") | LayerMask.GetMask("NPCHitbox")  | LayerMask.GetMask("Player") | LayerMask.GetMask("PlayerCollider") | LayerMask.GetMask("PlayerHitbox") | LayerMask.GetMask("UmbrellaCollider")));

        Debug.DrawLine(transform.position + Vector3.up * (agent.height / 2f), player.transform.position + Vector3.up * 0.8f, canSeePlayer ? Color.green : Color.red);

        if (!canSeePlayer)
            ChangeCurrentBehavior(idleBehavior);

        currentBehaviorName = currentBehavior.GetType().ToString(); // debug
        ExecuteBehavior(currentBehavior);

        if (useRigidBody)
        {
            rb.linearVelocity = AdjustForWall(rb.linearVelocity);
        }
    }

    public EnemyAI Spawn(Transform spawnPos)
    {
        return Instantiate(this, spawnPos.position, spawnPos.rotation);
    }

    public void DealDamage(float damage)
    {
        if (immune) return;

        immune = true;
        health -= damage;
        animator.SetTrigger("Damaged");

        if (health <= 0)
            StartCoroutine(Kill());

        if (getsStunned)
        {
            ChangeCurrentBehavior(stunnedBehavior);
        }
        else
        {
            ChangeCurrentBehavior(idleBehavior);
        }

        if (takesKnockback)
            ChangeVelocity((-transform.forward + (Vector3.up * 0.25f)).normalized * PlayerStats.Instance.knockback);


        StartCoroutine(ResetImmunity());
    }

    public void ChangeVelocity(Vector3 velocity)
    {
        StartCoroutine(UseRigidBody(velocity));
    }

    private IEnumerator UseRigidBody(Vector3 vector)
    {
        yield return null;
        agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.AddForce(vector, ForceMode.VelocityChange);

        yield return new WaitForFixedUpdate();

        useRigidBody = true;

        yield return new WaitUntil(
            () => rb.linearVelocity.magnitude < 0.1f
        );
        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        agent.Warp(transform.position);
        agent.enabled = true;
        useRigidBody = false;

        yield return null;
    }

    Vector3 AdjustForWall(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.01f) return velocity;

        Vector3 p1 = rb.position + Vector3.up * (0.3f);
        Vector3 p2 = rb.position + Vector3.up * (agent.height - 0.3f);

        float castDistance = Mathf.Max(0.25f, velocity.magnitude * Time.fixedDeltaTime);
        if (Physics.CapsuleCast(
            p1,
            p2,
            0.175f,
            velocity.normalized,
            out RaycastHit wallHit,
            castDistance,
            Util.nonColliderMasks
        ))
        {
            Vector3 normal = wallHit.normal;
            normal.y = 0;
            Vector3 projected = Vector3.ProjectOnPlane(velocity, normal);
            return projected;
        }

        return velocity;
    }

    public IEnumerator Kill()
    {
        currentBehavior = null;
        data.Clear();
        animator.SetTrigger("Died");
        agent.enabled = false;
        dead = true;

        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(2).IsName("Die"));

        yield return new WaitForSeconds(1.5f);

        Destroy(gameObject);
        StopAllCoroutines();
    }

    private IEnumerator ResetImmunity()
    {
        yield return new WaitForSeconds(immunityTime);
        immune = false;
    }

    public void ChangeCurrentBehavior(ScriptableObject newBehavior)
    {
        LeaveBehavior(currentBehavior);
        currentBehavior = newBehavior;
        InitializeBehavior(currentBehavior);
    }

    void InitializeBehavior(ScriptableObject behavior)
    {
        if (behavior != null && behavior is IEnemyBehavior enemyBehavior)
        {
            enemyBehavior.Initialize(this);
        }
    }

    void ExecuteBehavior(ScriptableObject behavior)
    {
        if (behavior != null && behavior is IEnemyBehavior enemyBehavior)
        {
            enemyBehavior.Execute(this);
        }
    }

    void LeaveBehavior(ScriptableObject behavior)
    {
        if (behavior != null && behavior is IEnemyBehavior enemyBehavior)
        {
            enemyBehavior.OnLeave(this);
        }
    }
}
