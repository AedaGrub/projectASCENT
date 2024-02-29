using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region HEALTH
    [SerializeField] private Slider[] healthSliders;
    [SerializeField] private Slider[] easeHealthSliders;
    private float maxHealth;
    private float currentHealth;

    #endregion

    #region ATTACK
    [SerializeField] private Image[] attackBars;
    #endregion

    #region COOLDOWN
    [SerializeField] private Slider[] cooldownSliders;
    #endregion

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (Slider slider in healthSliders)
        {
            slider.value = currentHealth;
        }
        foreach (Slider slider in easeHealthSliders)
        {
            slider.value = currentHealth;
        }
    }

    void Start()
    {
        //GET GM'S STATS AND REFRESH
        maxHealth = gameManager.instance.maxHealth;
        currentHealth = gameManager.instance.CurrentHealth;
        UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {

    }

    public void UpdateHealth(float newHP, float maxHP)
    {
        maxHealth = maxHP;
        float oldHP = currentHealth;
        currentHealth = newHP;

        //ENABLE/DISABLE HEALTH BARS
        for (int i = 0; i < healthSliders.Length; i++)
        {
            if (i * 10 < maxHealth)
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
            slider.value = currentHealth;
        }

        StartCoroutine(EaseHealth(oldHP, newHP));
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
