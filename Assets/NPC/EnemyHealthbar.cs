using System;
using TMPro;
using UnityEngine;

public class EnemyHealthbar : MonoBehaviour
{
    public EnemyAI enemyAI;
    public TextMeshProUGUI text;

    void Update()
    {
        float currentHealth = Math.Clamp(enemyAI.health, 0, enemyAI.maxHealth);
        text.text = $"{MathF.Round(currentHealth)}/{enemyAI.maxHealth}";

        Vector3 rot = enemyAI.player.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-rot);
    }
}
