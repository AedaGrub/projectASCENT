using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class introManager : MonoBehaviour
{
    public static introManager instance;

    public int sceneIndexToLoad;

    [SerializeField] private GameObject title;
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private GameObject cameraIntroTarget;
    [SerializeField] private GameObject player;
    public Animator cooldownNode;
    public Animator boonBelt;
    public Animator progressBar;

    [SerializeField] private GameObject Space;
    [SerializeField] private GameObject SpaceText;
    [SerializeField] private GameObject Shift;
    [SerializeField] private GameObject ShiftText;

    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera panCamera;

    const string alphaStart = "boonsGreyIn";
    const string alphaExit = "boonsGreyOut";

    [Header("HINTS")]
    [SerializeField] private LayerMask hintCheckLayer;

    [SerializeField] private Transform hint1Check;
    [SerializeField] private Vector2 hint1CheckSize;
    private float elapsedTime1;

    [SerializeField] private Transform hint2Check;
    [SerializeField] private Vector2 hint2CheckSize;
    private float elapsedTime2;

    [Header("EXIT")]
    [SerializeField] private Transform exitTransform;
    [SerializeField] private Vector2 exitSize;

    private bool playerAwake;
    private bool canExitTitle;
    private bool canExitLevel = true;

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
        blackScreen.SetActive(true);
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
        yield return new WaitForSeconds(1f);
        cooldownNode.Play(alphaStart);
        boonBelt.Play(alphaStart);
        progressBar.Play(alphaStart);
        gameManager.instance.ResetStats();
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

        //HINT ONE JUMP
        if (Physics2D.OverlapBox(hint1Check.position, hint1CheckSize, 0, hintCheckLayer))
        {
            elapsedTime1 += Time.deltaTime;
            if (elapsedTime1 > 3f)
            {
                Space.GetComponent<Animator>().Play(alphaStart);
            }
            if (elapsedTime1 > 10f)
            {
                SpaceText.GetComponent<Animator>().Play(alphaStart);
            }
        }
        else
        {
            if (elapsedTime1 > 10f)
            {
                SpaceText.GetComponent<Animator>().Play(alphaExit);
            }
            if (elapsedTime1 > 3f)
            {
                Space.GetComponent<Animator>().Play(alphaExit);
            }
            elapsedTime1 = 0;
        }

        //HINT TWO ATTACK
        if (Physics2D.OverlapBox(hint2Check.position, hint2CheckSize, 0, hintCheckLayer))
        {
            elapsedTime2 += Time.deltaTime;
            if (elapsedTime2 > 3f)
            {
                Shift.GetComponent<Animator>().Play(alphaStart);
            }
            if (elapsedTime2 > 10f)
            {
                ShiftText.GetComponent<Animator>().Play(alphaStart);
            }
        }
        else
        {
            if (elapsedTime2 > 10f)
            {
                ShiftText.GetComponent<Animator>().Play(alphaExit);
            }
            if (elapsedTime2 > 3f)
            {
                Shift.GetComponent<Animator>().Play(alphaExit);
            }
            elapsedTime2 = 0;
        }

        //EXIT
        if (Physics2D.OverlapBox(exitTransform.position, exitSize, 0, hintCheckLayer) && canExitLevel)
        {
            canExitLevel = false;
            StartCoroutine(ExitLevel());
        }
    }

    private IEnumerator ExitLevel()
    {
        UIManager.instance.BlackScreenStart();

        yield return new WaitForSeconds(1f);
        audioManager.instance.Stop("Music");
        levelLoader.instance.LoadNextLevel(sceneIndexToLoad);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(hint1Check.position, hint1CheckSize);
        Gizmos.DrawWireCube(hint2Check.position, hint2CheckSize);
        Gizmos.DrawWireCube(exitTransform.position, exitSize);
    }
}