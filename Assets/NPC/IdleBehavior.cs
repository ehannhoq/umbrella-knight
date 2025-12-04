using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IdleBehavior", menuName = "EnemyBehaviors/IdleBehavior")]
public class IdleBehavior : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject moveBehavior;
    public ScriptableObject attackBehavior;

    public float randomPositionDelay;
    private Coroutine positionVariation;


    public void Initialize(EnemyAI enemyAI) {}

    public void Execute(EnemyAI enemyAI)
    {
        Debug.Log("Idle!");

        
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





        if (positionVariation == null)
        {
            positionVariation = enemyAI.StartCoroutine(RandomPosition(enemyAI));
        }
    }

    IEnumerator RandomPosition(EnemyAI enemy)
    {
        Vector3 pos = enemy.transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        enemy.MoveToPosition(pos);

        yield return new WaitForSeconds(randomPositionDelay);
        positionVariation = null;
    }
}