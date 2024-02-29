using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region SLIDERS
    [SerializeField] private Slider[] healthSliders;
    [SerializeField] private Slider[] easeHealthSliders;

    [SerializeField] private Image[] attackBars;

    [SerializeField] private Slider[] cooldownSliders;
    [SerializeField] private Image cooldownIcon;
    #endregion

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //GET GM'S STATS AND REFRESH
        UpdateHealth();
        UpdateAttack();
        UpdateCooldown();
    }

    void Update()
    {
        //CONSTANTLY LOOK AT CURRENTCHARGEVALUE FOR COOLDOWN SLIDERS
        foreach (Slider slider in cooldownSliders)
        {
            slider.value = gameManager.instance.currentChargeValue;
        }
    }

    public void UpdateHealth()
    {
        //RENDERING HEALTH UI
        float oldHP = healthSliders[0].value;
        float newHP = gameManager.instance.CurrentHealth;

        //ENABLE/DISABLE HEALTH BARS
        for (int i = 0; i < healthSliders.Length; i++)
        {
            if (i * 10 < gameManager.instance.maxHealth)
            {
                healthSliders[i].gameObject.SetActive(true);
                easeHealthSliders[i].gameObject.SetActive(true);
            }
            else
            {
                healthSliders[i].gameObject.SetActive(false);
                easeHealthSliders[i].gameObject.SetActive(false);
            }
        }

        //SET VALUE TO ALL HEALTHSLIDERS
        foreach (Slider slider in healthSliders)
        {
            slider.value = gameManager.instance.CurrentHealth;
        }

        StartCoroutine(EaseHealth(oldHP, newHP));
    }

    public void UpdateAttack()
    {
        //RENDERING ATTACK UI
        for (int i = 0; i < attackBars.Length; i++)
        {
            if (i * 1 < gameManager.instance.currentAttack)
            {
                attackBars[i].gameObject.SetActive(true);
            }
            else
            {
                attackBars[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateCooldown()
    {
        //RENDERING COOLDOWN CHARGES UI
        for (int i = 0; i < cooldownSliders.Length; i++)
        {
            if (i * 20 < gameManager.instance.maxChargeValue)
            {
                cooldownSliders[i].gameObject.SetActive(true);
            }
            else
            {
                cooldownSliders[i].gameObject.SetActive(false);
            }
        }

        if (gameManager.instance.currentChargeValue > 0)
        {
            cooldownIcon.enabled = true;
        }
        else
        {
            cooldownIcon.enabled = false;
        }
    }

    private IEnumerator EaseHealth(float oldHealth, float newHealth)
    {
        yield return new WaitForSeconds(0.5f);
        float elapsedTime = 0;
        while (elapsedTime < 0.3f)
        {
            elapsedTime += Time.deltaTime;

            //SET VALUE TO ALL EASEHEALTHSLIDERS
            foreach (Slider slider in easeHealthSliders)
            {
                float lerpedEaseValue = Mathf.Lerp(oldHealth, newHealth, (elapsedTime / 0.3f));
                slider.value = lerpedEaseValue;
            }
            yield return null;
        }
    }
}
