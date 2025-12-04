using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WizardAttackBehavior", menuName = "EnemyBehaviors/Wizard/Attack")]
public class WizardAttack : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject idleBehavior;

    public void Initialize(EnemyAI enemyAI) { }
    public void Execute(EnemyAI enemyAI)
    {
        Debug.Log("Attack!");

        enemyAI.transform.rotation = Quaternion.LookRotation(enemyAI.player.transform.position - enemyAI.transform.position);

        if (enemyAI.distanceToPlayer > enemyAI.minimumAttackDistance)
        {
            enemyAI.ChangeCurrentBehavior(idleBehavior);
            return;
        }
    }
}