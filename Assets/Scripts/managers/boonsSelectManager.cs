using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class boonsSelectManager : MonoBehaviour
{
    public static boonsSelectManager instance;

    [SerializeField] private Animator greyBG;
    const string greyStart = "boonsGreyIn";
    const string greyExit = "boonsGreyout";

    public GameObject[] options;

    [SerializeField] private GameObject boonsParent;
    public GameObject[] boons;
    public GameObject boonBelt;

    public GameObject lastSelected {  get; set; }
    public int lastSelectedIndex { get; set; }

    public bool isSelectingOption;
    public bool isSelectingBelt;

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
        StartCoroutine(StartOptionSelectPhase());
    }

    public IEnumerator StartOptionSelectPhase()
    {
        yield return new WaitForSeconds(1);
        //RESET POSITION OF UI COMPONENTS
        boonsParent.transform.localPosition = new Vector2(0, 500);
        foreach (GameObject op in options)
        {
            op.transform.parent.localPosition = new Vector2(0, 0);
            op.SetActive(false);
        }

        //FADE IN GREY BG
        greyBG.Play(greyStart);

        //MOVE COMPONENTS
        LeanTween.moveLocal(boonsParent, new Vector3(0, 0, 0), 1f).setEaseOutExpo();

        yield return new WaitForSeconds(0.5f);
        LeanTween.moveLocal(options[0].transform.parent.gameObject, new Vector3(-190, 0, 0), 0.5f).setEaseOutExpo();
        LeanTween.moveLocal(options[1].transform.parent.gameObject, new Vector3(190, 0, 0), 0.5f).setEaseOutExpo();

        yield return new WaitForSeconds(0.5f);
        foreach (GameObject op in options)
        {
            op.SetActive(true);
        }

        yield return null;
    }

    private void ResetSelected()
    {
        StartCoroutine(SetSelectedAfterOneFrame());
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            HandleNextOptionSelection(1);
        }

        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            HandleNextOptionSelection(-1);
        }
    }

    private IEnumerator SetSelectedAfterOneFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HandleNextOptionSelection (int addition)
    {
        if (EventSystem.current.currentSelectedGameObject == null && lastSelected != null)
        {
            int newIndex = lastSelectedIndex + addition;
            newIndex = Mathf.Clamp(newIndex, 0, options.Length - 1);
            EventSystem.current.SetSelectedGameObject(options[newIndex]);
        }
    }

    private void HandleNextBoonSelection (int addition)
    {
        if (EventSystem.current.currentSelectedGameObject == null && lastSelected != null)
        {
            int newIndex = lastSelectedIndex + addition;
            newIndex = Mathf.Clamp(newIndex, 0, boons.Length - 1);
            EventSystem.current.SetSelectedGameObject(boons[newIndex]);
        }
    }

    
}
