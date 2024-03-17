using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseManager : MonoBehaviour
{
    public static baseManager instance;

    [SerializeField] private GameObject heart;
    [SerializeField] private GameObject spawnPoint;
    public GameObject lift;

    public GameObject boonBelt0;
    public GameObject boonBelt1;
    public GameObject boonBelt2;

    public GameObject health1;
    public GameObject health2;

    public GameObject dash;
    public GameObject doubleJump;

    public bool canInteract;
    public Transform interactTransform;
    public Vector2 interactSize;
    public LayerMask playerLayer;

    public ParticleSystem suctionPS;
    public ParticleSystem explodePS;

    public GameObject keyIcon;

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
        StartCoroutine(HoverHeart());
        float x = (0.1f * gameManager.instance.rewardedLevel);
        heart.transform.localScale = new Vector2(x, x);

        if (gameManager.instance.progressLevel > gameManager.instance.rewardedLevel)
        {
            canInteract = true;
            lift.SetActive(false);
        }

        for (int i = 0; i < gameManager.instance.progressLevel; i++)
        {
            if (i > (gameManager.instance.rewardedLevel - 1))
            {
                //RewardLevel(i);
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
            case 1:
                Instantiate(boonBelt0, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 1;
                break;
            case 2:
                Instantiate(health1, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 2;
                break;
            case 3:
                Instantiate(dash, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 3;
                break;
            case 4:
                Instantiate(boonBelt1, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 4;
                break;
            case 5:
                Instantiate(health2, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 5;
                break;
            case 6:
                Instantiate(doubleJump, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 6;
                break;
            case 7:
                Instantiate(boonBelt2, spawnPoint.transform.position, Quaternion.identity);
                gameManager.instance.rewardedLevel = 7;
                break;
        }
    }

    void Update()
    {
        if (Physics2D.OverlapBox(interactTransform.position, interactSize, 0, playerLayer))
        {
            if (canInteract && Input.GetKeyDown(KeyCode.W))
            {
                canInteract = false;
                StartCoroutine(FeedHeart());
            }
            StartCoroutine(FadeKey());
        }
        else
        {
            StartCoroutine(FadeKey());
        }
    }

    public IEnumerator FeedHeart()
    {
        suctionPS.Play();
        audioManager.instance.Play("heartRumble");
        cameraManager.instance.CameraShakeLight();
        UIManager.instance.ShrinkProgressScale();
        yield return new WaitForSeconds(4.7f);

        audioManager.instance.Play("heartGift");
        float x= (0.1f * gameManager.instance.progressLevel);
        heart.transform.localScale = new Vector2(x, x);
        explodePS.Play();
        cameraManager.instance.CameraShakeHeavy();
        gameManager.instance.IncreaseEXPCap();
        RewardLevel(gameManager.instance.progressLevel);
        yield return new WaitForSeconds(7);

        lift.SetActive(true);
    }

    private IEnumerator FadeKey()
    {
        float alphaStart = keyIcon.GetComponent<SpriteRenderer>().material.GetFloat("_Alpha");
        float alphaEnd;

        if (canInteract)
        {
            alphaEnd = 1;
        }
        else
        {
            alphaEnd = 0;
        }

        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float alphaNow = Mathf.Lerp(alphaStart, alphaEnd, (elapsedTime / 0.1f));

            keyIcon.GetComponent<SpriteRenderer>().material.SetFloat("_Alpha", alphaNow);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(interactTransform.position, interactSize);
    }
}
