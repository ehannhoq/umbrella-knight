using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Health")]
    public float health;
    public float maxHealth;

    [Header("Movement")]
    public float movementSpeed;

    [Header("Umbrella")]
    public float attackDamage;
    public float knockback;

    public event Action onPlayerHurt;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        health = maxHealth;
    }

    public void DealDamage(float damage)
    {
        health -= damage;

        onPlayerHurt.Invoke();

        if (health <= 0)
        {
            OnDeath();
        }
    }

    void OnDeath()
    {

    }
}
