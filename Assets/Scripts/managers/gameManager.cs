using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    #region REFERENCES
    [SerializeField] private GameObject player;
    [SerializeField] private playerController PlayerController;
    #endregion

    #region PLAYER HEALTH
    [SerializeField] public float maxHealth;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;
    #endregion

    #region PLAYER ATTACK

    #endregion

    #region PLAYER COOLDOWN

    #endregion

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        PlayerController = player.GetComponent<playerController>();
    }

    private void Update()
    {

    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void OnHit(float damageAmount)
    {
        currentHealth -= damageAmount;
        float newHealth = currentHealth;

        if (currentHealth <= 0)
        {
            //Die();
        }

        UIManager.instance.UpdateHealth(newHealth, maxHealth);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
