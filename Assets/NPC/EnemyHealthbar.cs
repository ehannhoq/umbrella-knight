using System;
using TMPro;
using UnityEngine;

public class EnemyHealthbar : MonoBehaviour
{
    public EnemyAI enemyAI;
    public TextMeshProUGUI text;

    void Update()
    {
        text.text = $"{MathF.Round(enemyAI.health)}/{enemyAI.maxHealth}";

        Vector3 rot = enemyAI.player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-rot);
    }
}
