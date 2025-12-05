using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    void OnTriggerEnter(Collider other)
    {
        enemyAI.DealDamage(PlayerStats.Instance.attackDamage);
    }
}
