using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelLoader : MonoBehaviour
{
    public static levelLoader instance;

    [SerializeField] private Animator transition;

    public bool isTransitioning;
    private int transitionType;
    private string transitionIn;
    private string transitionOut;

    #region TRANSITION TYPES
    const string inNorth = "slideInNorth";
    const string inSouth = "slideInSouth";
    const string inEast = "slideInEast";
    const string inWest = "slideInWest";

    const string outNorth = "slideOutNorth";
    const string outSouth = "slideOutSouth";
    const string outEast = "slideOutEast";
    const string outWest = "slideOutWest";
    #endregion

    void Awake()
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

        transition.gameObject.SetActive(true);
    }

    public void LoadNextLevel(int index)
    {
        StartCoroutine(LoadLevel(index));
        boonUIBelt.instance.InitialiseBoonBeltUI(1);
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transitionType = Random.Range(0, 4);
        isTransitioning = true;
        Transition("Start");

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(levelIndex);

        gameManager.instance.currentRoom++;
        UIManager.instance.UpdateReferences();
        UIManager.instance.UpdateRoomUI();
        gameManager.instance.FindReferences();

        yield return new WaitForSeconds(0.5f);

        Transition("Exit");
        isTransitioning = false;
    }

    private void Transition(string state)
    {
        if (transitionType == 0)
        {
            transitionIn = inNorth;
            transitionOut = outSouth;
        }
        else if (transitionType == 1)
        {
            transitionIn = inSouth;
            transitionOut = outNorth;
        }
        else if (transitionType == 2)
        {
            transitionIn = inEast;
            transitionOut = outWest;
        }
        else if (transitionType == 3)
        {
            transitionIn = inWest;
            transitionOut = outEast;
        }

        if (state == "Start")
        {
            transition.Play(transitionIn);
        }
        else
        {
            transition.Play(transitionOut);
        }
    }
}
