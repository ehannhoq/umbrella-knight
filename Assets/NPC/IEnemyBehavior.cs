using UnityEngine;

public interface IEnemyBehavior
{
    void Initialize(EnemyAI enemyAI);
    void Execute(EnemyAI enemyAI);
}