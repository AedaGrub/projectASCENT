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
    [SerializeField] private Material playerMaterial;
    #endregion

    #region PLAYER HEALTH
    [Header("HEALTH")]
    [SerializeField] private float defaultHealth;
    [SerializeField] public float maxHealth;
    [SerializeField] private float currentHealth;
    public float CurrentHealth => currentHealth;

    public bool isInvincible;
    [SerializeField] private float invulnDuration;
    #endregion

    #region PLAYER ATTACK
    [Header("ATTACK")]
    [SerializeField] private float defaultAttack;
    [SerializeField] public float currentAttack;
    #endregion

    #region PLAYER COOLDOWN
    [Header("COOLDOWN CHARGES")]
    [SerializeField] public float currentChargeValue;
    [SerializeField] public float defaultChargeRate;
    [SerializeField] public float chargeRate;
    #endregion

    #region PLAYER SHIELD
    [Header("SHIELD")]
    [SerializeField] public bool haveShield;
    [SerializeField] public float currentShieldValue;
    [SerializeField] private float defaultShieldRate;
    [SerializeField] public float currentShieldRate;
    private bool playedShieldRefresh;
    #endregion

    #region PLAYER PERMISSIONS
    [Header("PLAYER PERMISSIONS")]
    public bool playerEnabled;
    public bool canExtraJump;
    public bool canDash;
    public bool canWallClimb;
    public bool canBeHit;
    #endregion

    #region FLOORS AND ROOMS
    public int currentFloor;
    public int currentRoom;
    #endregion

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        FindReferences();
        ResetStats();
    }

    public void FindReferences()
    {
        player = GameObject.FindWithTag("Player");
        PlayerController = player.GetComponent<playerController>();
        playerMaterial = player.GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            playerEnabled = !playerEnabled;
        }

        //RECHARGE CHARGES AT RATE
        if (currentChargeValue < 1)
        {
            currentChargeValue += chargeRate * Time.deltaTime;
        }
        else if (currentChargeValue >= 1)
        {
            currentChargeValue = 1;
        }

        //RECHARGE SHIELDS AT RATE
        if (currentShieldValue < 1 && haveShield)
        {
            playedShieldRefresh = false;
            currentShieldValue += currentShieldRate * Time.deltaTime;
        }
        else if (currentShieldValue >= 1 && haveShield)
        {
            if (!playedShieldRefresh)
            {
                playedShieldRefresh = true;
                PlayerController.ShieldRefreshPS();
            }
            currentShieldValue = 1;
        }
        else if (!haveShield)
        {
            currentShieldValue = 0;
        }
    }

    public void ResetStats()
    {
        maxHealth = defaultHealth;
        currentHealth = defaultHealth;

        currentAttack = defaultAttack;

        chargeRate = defaultChargeRate;

        haveShield = false;
        currentShieldRate = defaultShieldRate;
    }

    public void OnHit(float damageAmount)
    {
        if (player == null)
        {
            FindReferences();
        }

        isInvincible = true;
        bool wasShielded = false;
        if (currentShieldValue >= 1)
        {
            wasShielded = true;
            currentShieldValue = 0;
        }
        else
        {
            currentHealth -= damageAmount;
        }
        UIManager.instance.UpdateHealth();

        if (currentHealth <= 0)
        {
            cameraManager.instance.CameraShakeHeavy();
            audioManager.instance.Stop("Music");
            //Die();
        }
        else
        {
            StartCoroutine(PlayerHurt(wasShielded));
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private IEnumerator PlayerHurt(bool wasShielded)
    {
        if (wasShielded)
        {
            PlayerController.ShieldHurtPS();
        }
        else
        {
            PlayerController.PlayerHurtPS();
        }

        cameraManager.instance.CameraShakeLight();

        yield return StartCoroutine(Sleep(0.15f));

        StartCoroutine(InvulnFrames());
    }

    private IEnumerator Sleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    private IEnumerator InvulnFrames()
    {
        float elapsedTime = 0;

        while (elapsedTime < invulnDuration)
        {
            //FLASHING
            elapsedTime += Time.deltaTime;
            if (playerMaterial.GetFloat("_Alpha") == 1f)
            {
                playerMaterial.SetFloat("_Alpha", 0);
            }
            else
            {
                playerMaterial.SetFloat("_Alpha", 1);
            }
            yield return null;
        }
        playerMaterial.SetFloat("_Alpha", 1);
        isInvincible = false;
    }

    public void IFrames()
    {
        StartCoroutine(DodgeFrames());
    }

    private IEnumerator DodgeFrames()
    {
        if (PlayerController.isDashAttacking)
        {
            isInvincible = true;
            while (isInvincible)
            {
                if (playerMaterial.GetFloat("_Alpha") == 1f)
                {
                    playerMaterial.SetFloat("_Alpha", 0.5f);
                }
                else
                {
                    playerMaterial.SetFloat("_Alpha", 1);
                }
                yield return null;
            }
        }
        else
        {
            playerMaterial.SetFloat("_Alpha", 1);
            isInvincible = false;
        }
    }
}
