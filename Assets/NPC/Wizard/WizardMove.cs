using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

[CreateAssetMenu(fileName = "WizardMoveBehavior", menuName = "EnemyBehaviors/Wizard/Move")]
public class WizardMove : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject idleBehavior;
    public ScriptableObject attackBehavior;

    public void Initialize(EnemyAI enemyAI)
    {
        enemyAI.animator.SetBool("Walking", true);

        enemyAI.agent.Warp(enemyAI.transform.position);
        enemyAI.agent.enabled = true;
    }

    public void Execute(EnemyAI enemyAI)
    {
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

        if (enemyAI.agent.enabled)
            enemyAI.agent.SetDestination(enemyAI.player.transform.position);
    }

    public void OnLeave(EnemyAI enemyAI)
    {
        enemyAI.animator.SetBool("Walking", false);
    }
}