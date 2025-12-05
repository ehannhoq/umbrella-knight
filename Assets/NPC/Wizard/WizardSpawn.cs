using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WizardSpawnBehavior", menuName = "EnemyBehaviors/Wizard/Spawn")]
public class WizardSpawn : ScriptableObject, IEnemyBehavior
{
    public void Initialize(EnemyAI enemyAI)
    {
        int staffIndex = Random.Range(0, 1);
        Debug.Log("Spawned wizard with staff index: " + staffIndex);
        
        Transform staffs = enemyAI.transform.Find("root/pelvis/Weapon");

        GameObject staff = staffs.GetChild(staffIndex).gameObject;
        staff.SetActive(true);

        enemyAI.data["staff_type"] = staffIndex;
        enemyAI.data["staff"] = staff;

    }
    public void Execute(EnemyAI enemyAI) { }
    public void OnLeave(EnemyAI enemyAI) { }
}