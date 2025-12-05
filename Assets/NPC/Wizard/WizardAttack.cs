using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WizardAttackBehavior", menuName = "EnemyBehaviors/Wizard/Attack")]
public class WizardAttack : ScriptableObject, IEnemyBehavior
{
    public ScriptableObject idleBehavior;
    public GameObject[] projectiles;
    public float[] attackTimes;

    public void Initialize(EnemyAI enemyAI)
    {
        enemyAI.animator.SetTrigger("ResetAttack");
        enemyAI.animator.ResetTrigger("Attack");

        enemyAI.data["isAttacking"] = false;
        enemyAI.data["attackCoroutine"] = null;
        
        StartAttack(enemyAI);
    }

    public void Execute(EnemyAI enemyAI)
    {
        Vector3 dir = enemyAI.player.transform.position - enemyAI.transform.position;
        dir.y = 0;
        enemyAI.transform.rotation = Quaternion.LookRotation(dir);

        if ((bool)enemyAI.data["isAttacking"]) return;

        if (enemyAI.distanceToPlayer > enemyAI.minimumAttackDistance)
        {
            enemyAI.ChangeCurrentBehavior(idleBehavior);
            return;
        }

        StartAttack(enemyAI);
    }

    public void OnLeave(EnemyAI enemyAI)
    {
        if (enemyAI.data["attackCoroutine"] != null)
        {
            enemyAI.StopCoroutine((Coroutine)enemyAI.data["attackCoroutine"]);
            enemyAI.data["attackCoroutine"] = null;
        }
        enemyAI.data["isAttacking"] = false;
        enemyAI.animator.SetTrigger("ResetAttack");
        enemyAI.animator.ResetTrigger("Attack");
    }


    private void StartAttack(EnemyAI enemyAI)
    {
        if (enemyAI.data["attackCoroutine"] != null) return;
        enemyAI.data["attackCoroutine"] = enemyAI.StartCoroutine(AttackSequence(enemyAI));
    }


    private IEnumerator AttackSequence(EnemyAI enemyAI)
    {
        enemyAI.data["isAttacking"] = true;

        enemyAI.agent.enabled = false;
        enemyAI.animator.SetTrigger("Attack");

        yield return new WaitUntil(() =>
            enemyAI.animator.GetCurrentAnimatorStateInfo(1).IsName("Attack02Maintain"));

        yield return new WaitUntil(() =>
            enemyAI.animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.4f);

        FireProjectile(enemyAI);

        yield return new WaitUntil(() =>
            enemyAI.animator.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f);

        enemyAI.animator.SetTrigger("ResetAttack");
        enemyAI.animator.ResetTrigger("Attack");

        float cooldown = attackTimes[(int)enemyAI.data["staff_type"]];
        yield return new WaitForSeconds(cooldown);

        enemyAI.data["isAttacking"] = false;
        enemyAI.data["attackCoroutine"] = null;
        enemyAI.agent.enabled = true;
    }


    private void FireProjectile(EnemyAI enemyAI)
    {
        int type = (int)enemyAI.data["staff_type"];
        GameObject prefab = projectiles[type];

        if (!prefab.TryGetComponent(out Projectile proj)) return;

        GameObject staff = enemyAI.data["staff"] as GameObject;
        Vector3 pos = staff.transform.position + Vector3.up;

        Quaternion rot = Quaternion.LookRotation(
            (enemyAI.player.transform.position + (Vector3.up * 0.6f)) - pos
        );

        proj.SpawnProjectile(pos, rot);
    }
}