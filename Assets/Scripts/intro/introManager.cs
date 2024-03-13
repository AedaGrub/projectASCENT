using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class introManager : MonoBehaviour
{
    public static introManager instance;

    [SerializeField] private GameObject title;
    [SerializeField] private GameObject cameraIntroTarget;
    [SerializeField] private GameObject player;

    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera panCamera;

    const string alphaStart = "boonsGreyIn";
    const string alphaExit = "boonsGreyOut";

    private bool playerAwake;
    private bool canExitTitle;

    private void Awake()
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
        cameraManager.instance.SwapCamera(playerCamera, panCamera, 1);
        StartCoroutine(TitleScreen());
        player.GetComponent<playerController>().Sleep();
    }

    private IEnumerator TitleScreen()
    {
        //SHOW TITLE
        title.GetComponent<Animator>().Play(alphaStart);
        yield return new WaitForSeconds(2f);

        //HIDE BLACKSCREEN AND PLAY MUSIC
        UIManager.instance.BlackScreenExit();
        audioManager.instance.Play("Respite");
        yield return new WaitForSeconds(2f);

        //PAN TO PLAYER
        LeanTween.moveLocal(cameraIntroTarget, new Vector3(1, 2, 0), 5f).setEaseInOutSine();
        yield return new WaitForSeconds(5f);

        canExitTitle = true;
    }

    private IEnumerator CloseTitle()
    {
        //SWIPE AND HIDE TITLE
        LeanTween.moveLocal(title, new Vector3(0, 300, 0), 1f).setEaseInExpo();
        title.GetComponent<Animator>().Play(alphaExit);

        //WOBBLE PLAYER, THEN STAND UP AND CHANGE TO PLAYER CAMERA
        LeanTween.moveLocal(player, new Vector3(0.1f, player.transform.position.y, player.transform.position.z), 0.05f).setEaseLinear();
        yield return new WaitForSeconds(0.05f);
        LeanTween.moveLocal(player, new Vector3(-0.1f, player.transform.position.y, player.transform.position.z), 0.1f).setEaseLinear();
        yield return new WaitForSeconds(0.1f);
        LeanTween.moveLocal(player, new Vector3(0f, player.transform.position.y, player.transform.position.z), 0.1f).setEaseLinear();
        yield return new WaitForSeconds(1f);
        player.GetComponent<playerController>().StandUp();
        yield return new WaitForSeconds(0.5f);
        gameManager.instance.EnablePlayer();

        cameraManager.instance.SwapCamera(panCamera, playerCamera, 1);
        yield return null;
    }

    void Update()
    {
        if (!playerAwake && canExitTitle)
        {
            if (Input.anyKey)
            {
                playerAwake = true;
                StartCoroutine(CloseTitle());
            }
        }
    }
}
