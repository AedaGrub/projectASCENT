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

    [SerializeField] private GameObject _enemyHitFX;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnHit(float damageAmount, Vector2 knockbackAmount)
    {
        currentHealth -= damageAmount;
        rb.AddForce(knockbackAmount, ForceMode2D.Impulse);

        objectPoolManager.SpawnObject(_enemyHitFX, transform.position, 
            Quaternion.Euler(0.0f, 0.0f, Random.Range(-20.0f, 0.0f)), objectPoolManager.PoolType.ParticleSystem);

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
