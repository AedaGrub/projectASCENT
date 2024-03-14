using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseManager : MonoBehaviour
{
    public static baseManager instance;

    [SerializeField] private GameObject heart;
    [SerializeField] private GameObject spawnPoint;

    public GameObject boonBelt0;
    public GameObject boonBelt1;
    public GameObject boonBelt2;

    public GameObject dash;
    public GameObject doubleJump;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        float scale = 0.1f * (1 + (gameManager.instance.totalProgressEXP / 20));
        heart.transform.localScale = new Vector2(scale, scale);
        StartCoroutine(HoverHeart());

        for (int i = 0; i < gameManager.instance.progressLevel; i++)
        {
            if (i > (gameManager.instance.rewardedLevel - 1))
            {
                RewardLevel(i);
            }
        }

        gameManager.instance.ResetStats();
        UIManager.instance.BlackScreenExit();

        if (gameManager.instance.progressLevel >= 1)
        {
            if (gameManager.instance.progressLevel >= 3)
            {
                if (gameManager.instance.progressLevel >= 5)
                {
                    audioManager.instance.Play("Bar");
                }
                else
                {
                    audioManager.instance.Play("Respite");
                }
            }
            else
            {
                audioManager.instance.Play("Intermission");
            }
        }
    }

    private IEnumerator HoverHeart()
    {
        while (true)
        {
            LeanTween.moveLocal(heart, new Vector3(0.18f, 0f, -3), 1f).setEaseInOutSine();
            yield return new WaitForSeconds(1f);
            LeanTween.moveLocal(heart, new Vector3(0.18f, -0.04f, -3), 1f).setEaseInOutSine();
            yield return new WaitForSeconds(1f);
        }
    }

    private void RewardLevel(int level)
    {
        switch (level)
        {
            case 0:
                Instantiate(boonBelt0, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 1;
                break;
            case 1:
                Instantiate(dash, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 2;
                break;
            case 2:
                Instantiate(boonBelt1, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 3;
                break;
            case 3:
                Instantiate(doubleJump, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 4;
                break;
            case 4:
                Instantiate(boonBelt2, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 5;
                break;
        }
    }

    void Update()
    {

    }
}
