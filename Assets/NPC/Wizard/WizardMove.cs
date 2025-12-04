using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WizardMoveBehavior", menuName = "EnemyBehaviors/Wizard/Move")]
public class WizardMove : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject idleBehavior;
    public ScriptableObject attackBehavior;
    public float movementSpeed;

    public void Initialize(EnemyAI enemyAI)
    {
        enemyAI.agent.acceleration = movementSpeed;
        enemyAI.agent.speed = movementSpeed;
    }
    public void Execute(EnemyAI enemyAI)
    {
        Debug.Log("Move!");

        enemyAI.transform.rotation = Quaternion.LookRotation(enemyAI.player.transform.position - enemyAI.transform.position);
        
        if (enemyAI.distanceToPlayer <= enemyAI.minimumAttackDistance)
        {
            enemyAI.ChangeCurrentBehavior(attackBehavior);
            return;
        }

        if (enemyAI.distanceToPlayer > enemyAI.minimumMoveDistance)
        {
            enemyAI.ChangeCurrentBehavior(idleBehavior);
            return;
        }

        enemyAI.MoveToPosition(enemyAI.player.transform.position);
    }
}