using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region HEALTH
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider easeHealthSlider;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    [SerializeField] private float lerpSpeed;

    #endregion

    #region ATTACK

    #endregion

    #region COOLDOWN

    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed);
        }
    }

    public void UpdateHealth(float newHP, float maxHP)
    {
        maxHealth = maxHP;
        currentHealth = newHP;
        healthSlider.value = currentHealth;
    }
}
