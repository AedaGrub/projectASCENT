using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    #region REFERENCES
    [Header("REFERENCES")]
    [SerializeField] private GameObject player;
    [SerializeField] private playerController PlayerController;
    #endregion

    #region PLAYER HEALTH
    [Header("HEALTH")]
    [SerializeField] private float defaultHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;
    #endregion

    #region PLAYER ATTACK
    [Header("ATTACK")]
    [SerializeField] private float defaultAttack;
    [SerializeField] public float currentAttack;
    #endregion

    #region PLAYER COOLDOWN
    [Header("COOLDOWN CHARGES")]
    [SerializeField] private float defaultChargeValue;
    [SerializeField] public float maxChargeValue;
    [SerializeField] public float currentChargeValue;
    [SerializeField] public float defaultChargeRate;
    [SerializeField] public float chargeRate;
    #endregion

    #region PLAYER PERMISSIONS
    [Header("PLAYER PERMISSIONS")]
    public bool playerEnabled;
    public bool canExtraJump;
    public bool canDash;
    public bool canWallClimb;
    public bool canBeHit;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        PlayerController = player.GetComponent<playerController>();
        ResetStats();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            playerEnabled = !playerEnabled;
        }

        //RECHARGE CHARGES AT RATE
        if (currentChargeValue < maxChargeValue)
        {
            currentChargeValue += chargeRate;
        }
    }

    public void ResetStats()
    {
        maxHealth = defaultHealth;
        currentHealth = defaultHealth;

        currentAttack = defaultAttack;

        maxChargeValue = defaultChargeValue;
        currentChargeValue = defaultChargeValue;
    }

    public void OnHit(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            //Die();
        }

        UIManager.instance.UpdateHealth();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
