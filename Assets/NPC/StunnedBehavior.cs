using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "StunnedBehavior", menuName = "EnemyBehaviors/StunnedBehavior")]
public class StunnedBehavior : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject idleBehavior;

    public void Initialize(EnemyAI enemyAI)
    {
        enemyAI.StartCoroutine(Util.DelayedActionSeconds(enemyAI.stunDuration, () => { enemyAI.ChangeCurrentBehavior(idleBehavior); }));
    }

    public void Execute(EnemyAI enemyAI) {}

    public void OnLeave(EnemyAI enemyAI) { }
}
