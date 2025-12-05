using System;
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
    public float baseAttackDamage;
    public float attackDamage;
    public float knockback;

    public event Action onPlayerHurt;

    private Rigidbody _rb;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        health = maxHealth;
        _rb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        
    }


    void Update()
    {
        attackDamage = baseAttackDamage + _rb.linearVelocity.magnitude;
    }
}
