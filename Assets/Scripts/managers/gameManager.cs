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
    [SerializeField] private playerAttack PlayerAttack;
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

    [SerializeField] private float defaultRange;
    [SerializeField] public float currentRange;
    #endregion

    #region PLAYER DASH
    [Header("DASH")]
    [SerializeField] public float currentChargeValue;
    [SerializeField] public float defaultChargeRate;
    [SerializeField] public float chargeRate;
    private bool playedDashRefresh;

    [SerializeField] public float defaultDashSpeed;
    [SerializeField] public float currentDashSpeed;
    [SerializeField] public float iFramesDuration;
    #endregion

    #region PLAYER SHIELD
    [Header("SHIELD")]
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
        PlayerAttack = player.GetComponent<playerAttack>();
        playerMaterial = player.GetComponent<SpriteRenderer>().material;
    }

    public void EnablePlayer()
    {
        playerEnabled = true;
    }

    public void DisablePlayer()
    {
        playerEnabled = false;
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
            playedDashRefresh = false;
            currentChargeValue += chargeRate * Time.deltaTime;
        }
        else if (currentChargeValue >= 1)
        {
            if (!playedDashRefresh)
            {
                playedDashRefresh = true;
            }
            currentChargeValue = 1;
        }

        //RECHARGE SHIELDS AT RATE
        if (currentShieldValue < 1 && currentShieldRate > 0)
        {
            playedShieldRefresh = false;
            currentShieldValue += currentShieldRate * Time.deltaTime;
        }
        else if (currentShieldValue >= 1 && currentShieldRate > 0)
        {
            if (!playedShieldRefresh)
            {
                playedShieldRefresh = true;
                PlayerController.ShieldRefreshPS();
                audioManager.instance.Play("shieldRefresh");
            }
            currentShieldValue = 1;
        }
        else if (currentShieldRate <= 0)
        {
            currentShieldValue = 0;
        }
    }

    public void UpdateStats(boonSO boon, bool isAdding)
    {
        int value;
        float bonus;
        if (isAdding)
        {
            value = 1;
        }
        else
        {
            value = -1;
        }

        if (boon.type == "health")
        {
            bonus = ((2 + boon.tier) * value);
            maxHealth += bonus;
            if (value == -1)
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }
            else
            {
                currentHealth += bonus;
            }
            UIManager.instance.UpdateHealth();
        }

        if (boon.type == "attack")
        {
            bonus = ((1 + boon.tier) * value);
            currentAttack += bonus;
        }

        if (boon.type == "shield")
        {
            if (currentShieldValue <= 0)
            {
                currentShieldValue = 1;
            }
            bonus = ((0.05f + (0.02f * boon.tier)) * value);
            currentShieldRate += bonus;
        }

        if (boon.type == "dash")
        {
            bonus = ((0.05f + (0.02f * boon.tier)) * value);
            chargeRate += bonus;
            bonus = ((3f * boon.tier) * value);
            currentDashSpeed += bonus;
        }

        if (boon.type == "cooldown")
        {
            bonus = ((0.5f * boon.tier) * value);
            invulnDuration += bonus;
            iFramesDuration += bonus;
        }

        if (boon.type == "range")
        {
            bonus = ((0.2f * boon.tier) * value);
            currentRange += bonus;
        }
    }

    public void ResetStats()
    {
        maxHealth = defaultHealth;
        currentHealth = defaultHealth;

        currentAttack = defaultAttack;

        chargeRate = defaultChargeRate;
        currentDashSpeed = defaultDashSpeed;

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
        audioManager.instance.Play("playerHurt");

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
        if (player == null)
        {
            FindReferences();
        }
        StartCoroutine(DodgeFrames());
    }

    private IEnumerator DodgeFrames()
    {
        if (PlayerController.isDashAttacking)
        {
            isInvincible = true;
            while (isInvincible)
            {
                playerMaterial.SetFloat("_Alpha", 0.5f);
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(iFramesDuration);
            playerMaterial.SetFloat("_Alpha", 1);
            isInvincible = false;
        }
    }
}
