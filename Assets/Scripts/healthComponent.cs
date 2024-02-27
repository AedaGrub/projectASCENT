using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthComponent : MonoBehaviour, IDamageable
{
    public delegate void HealthChangedHandler (object source, float oldHealth, float newHealth);
    public event HealthChangedHandler OnHealthChanged;

    [SerializeField] public float maxHealth;
    [SerializeField] private float currentHealth;
    [HideInInspector] public float CurrentHealth => currentHealth;
    [SerializeField] private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void ChangeHealth(int amount)
    {
        float oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void OnHit(float damageAmount, Vector2 knockbackAmount)
    {
        currentHealth -= damageAmount;
        rb.AddForce(knockbackAmount, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
