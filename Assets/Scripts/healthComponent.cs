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
    [SerializeField] public bool isAerial;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject player;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material whiteMat;

    [SerializeField] private GameObject _enemyHitFX;
    [SerializeField] private GameObject _enemyBleedFX;
    [SerializeField] private GameObject _playerAtkFX;
    [SerializeField] private GameObject _enemyDeathFX;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseMat = sr.material;
        player = GameObject.FindWithTag("Player");
    }

    public void OnHit(float damageAmount, Vector2 knockbackAmount)
    {
        //DAMAGE
        currentHealth -= damageAmount;
        //SET WHITEMAT
        sr.material = whiteMat;

        //KNOCKBACK
        if (isAerial)
        {
            rb.AddForce(knockbackAmount, ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(new Vector2(knockbackAmount.x, 0f), ForceMode2D.Impulse);
        }

        //FX
        objectPoolManager.SpawnObject(_enemyHitFX, transform.position, 
            Quaternion.Euler(0.0f, 0.0f, Random.Range(-20.0f, 0.0f)), objectPoolManager.PoolType.ParticleSystem);

        Vector3 dir = (transform.position - player.transform.position).normalized;
        Quaternion spawnRot = Quaternion.FromToRotation(Vector2.right, dir);
        objectPoolManager.SpawnObject(_enemyBleedFX, transform.position,
            spawnRot, objectPoolManager.PoolType.ParticleSystem);

        spawnRot = Quaternion.FromToRotation(Vector2.right, dir * Random.Range(1f, 2f));
        objectPoolManager.SpawnObject(_playerAtkFX, new Vector3(transform.position.x, transform.position.y, -5f),
            spawnRot, objectPoolManager.PoolType.GameObject);


        if (currentHealth <= 0)
        {
            //DEATH
            cameraManager.instance.CameraShakeMed();
            spawnRot = Quaternion.FromToRotation(Vector2.up, dir);
            objectPoolManager.SpawnObject(_enemyDeathFX, transform.position, spawnRot, objectPoolManager.PoolType.ParticleSystem);
            Die();
        }
        else
        {
            //RESET WHITE MAT TO BASE MAT
            Invoke("ResetMaterial", 0.1f);
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void ResetMaterial()
    {
        sr.material = baseMat;
    }
}
