using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("EnemyHitbox: OnTriggerEnter with " + other.name);
        enemyAI.DealDamage(PlayerStats.Instance.attackDamage);
    }
}
