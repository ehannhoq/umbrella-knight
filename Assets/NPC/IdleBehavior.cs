using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleBehavior", menuName = "EnemyBehaviors/IdleBehavior")]
public class IdleBehavior : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject moveBehavior;
    public ScriptableObject attackBehavior;

    public void Initialize(EnemyAI enemyAI) {}

    public void Execute(EnemyAI enemyAI)
    {
        if (enemyAI.distanceToPlayer <= enemyAI.minimumAttackDistance)
        {
            enemyAI.ChangeCurrentBehavior(attackBehavior);
            return;
        }

        if (enemyAI.distanceToPlayer < enemyAI.minimumMoveDistance)
        {
            enemyAI.ChangeCurrentBehavior(moveBehavior);
            return;
        }
    }

    public void OnLeave(EnemyAI enemyAI) { }
}