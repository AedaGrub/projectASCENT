using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class boonsSelectManager : MonoBehaviour
{
    public static boonsSelectManager instance;

    [Header("BOON STAT")]
    [SerializeField] private boonSO currentBoon;
    private int boonTierInt;
    public int[] tier = { 1, 0, 0 };

    [Header("BOON TABLE")]
    public List<boonSO> tier0Pool;
    public List<boonSO> tier1Pool;
    public List<boonSO> tier2Pool;

    [Header("GREY BG")]
    [SerializeField] private Animator greyBG;
    const string greyStart = "boonsGreyIn";
    const string greyExit = "boonsGreyOut";

    [Header("ACTION OPTIONS")]
    public GameObject[] options;

    [Header("BOON OPTIONS")]
    [SerializeField] private GameObject boonsParent;
    public List<GameObject> boons = new List<GameObject>();
    public GameObject boonBelt;
    [SerializeField] private GameObject swapIcon;

    [Header("DISPLAY BOON")]
    public GameObject boonIcon;
    public GameObject boonTier;
    public GameObject boonFill;
    public GameObject boonMask;

    public GameObject lastSelected {  get; set; }
    public int lastSelectedIndex { get; set; }

    [Header("PERMISSIONS")]
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
    }

    public void UnlockTier1()
    {
        tier[0] = 1;
        tier[1] = 2;
    }

    public void UnlockTier2()
    {
        tier[1] = 3;
        tier[2] = 2;
    }

    public void StartOptionSelection()
    {
        //RANDOM TIER
        int tierTotal = 0;
        foreach (var item in tier)
        {
            tierTotal += item;
        }
        int randomNumber = Random.Range(0, tierTotal);
        var chosenTierPool = tier0Pool;
        for (int i = 0; i < tier.Length; i++)
        {
            if (randomNumber <= tier[i])
            {
                switch (i)
                {
                    case 0:
                        chosenTierPool = tier0Pool; 
                        break;
                    case 1:
                        chosenTierPool = tier1Pool;
                        break;
                    case 2:
                        chosenTierPool = tier2Pool;
                        break;
                }
            }
            else
            {
                randomNumber -= tier[i];
            }
        }

        //RANDOM BOON
        randomNumber = Random.Range(0, chosenTierPool.Count);
        for (int i = 0; i < chosenTierPool.Count; i++)
        {
            if (randomNumber == i)
            {
                currentBoon = chosenTierPool[i];
            }
        }

        //SET DISPLAY BOON
        boonIcon.GetComponent<Image>().sprite = currentBoon.iconSprite;
        boonTier.GetComponent<Image>().sprite = currentBoon.tierSprite;

        //EXECUTE OPTION SELECT PHASE
        StartCoroutine(StartOptionSelectPhase());
    }

    public IEnumerator StartOptionSelectPhase()
    {
        ResetSelected();

        //RESET  UI COMPONENTS
        boonsParent.transform.localScale = new Vector3(1, 1, 1);
        boonsParent.transform.localPosition = new Vector2(0, 500);
        boonMask.transform.localScale = new Vector3(1, 1, 1);
        foreach (GameObject op in options)
        {
            op.transform.parent.localPosition = new Vector2(0, 0);
            op.transform.parent.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            op.SetActive(false);
        }
        swapIcon.transform.localPosition = new Vector2(0, 0);
        foreach (GameObject b in boons)
        {
            b.GetComponent<Button>().onClick.AddListener(delegate { SelectBoon(); });
            b.GetComponent<Button>().enabled = false;
        }

        //FADE IN GREY BG AND DISABLE PLAYER
        greyBG.Play(greyStart);
        gameManager.instance.playerEnabled = false;

        //MOVE COMPONENTS
        LeanTween.moveLocal(boonsParent, new Vector3(0, 0, 0), 1f).setEaseOutExpo();

        yield return new WaitForSeconds(0.7f);
        LeanTween.moveLocal(options[0].transform.parent.gameObject, new Vector3(-190, 0, 0), 0.3f).setEaseOutExpo();
        LeanTween.moveLocal(options[1].transform.parent.gameObject, new Vector3(190, 0, 0), 0.3f).setEaseOutExpo();

        yield return new WaitForSeconds(0.3f);
        isSelectingOption = true;

        options[0].SetActive(true);
        options[1].SetActive(true);

        ResetSelected();

        yield return null;
    }

    public void ExitBoonSelection()
    {
        if (isSelectingOption)
        {
            StartCoroutine(ExitBoonSelect());
        }
    }

    private IEnumerator ExitBoonSelect()
    {
        ResetSelected();
        isSelectingOption = false;
        isSelectingBelt = false;

        //RESET AND DISABLE OPTIONS
        foreach (GameObject op in options)
        {
            op.transform.parent.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
            op.SetActive(false);
            LeanTween.moveLocal(op.transform.parent.gameObject, new Vector3(0, 0, 0), 0.3f).setEaseInExpo();
        }
        foreach (GameObject b in boons)
        {
            b.GetComponent<Button>().enabled = false;
        }

        float y;

        //RESET AND HIDE PARENT
        if (EventSystem.current.currentSelectedGameObject == options[0])
        {
            y = -500;
            LeanTween.rotate(boonsParent, new Vector3(0, 0, 720), 0.75f).setEaseInExpo();
        }
        else
        {
            y = 500;
        }
        LeanTween.moveLocal(boonsParent, new Vector3(0, y, 0), 0.5f).setEaseInExpo();

        //FADE OUT BG AND ENABLE PLAYER
        greyBG.Play(greyExit);
        gameManager.instance.playerEnabled = true;

        spawnManager.instance.EndLevel();

        yield return null;
    }

    public void StartBoonSelection()
    {
        if (isSelectingOption)
        {
            StartCoroutine(StartBoonSelectPhase());
        }
    }

    private IEnumerator StartBoonSelectPhase()
    {
        ResetSelected();
        isSelectingOption = false;
        //ENABLE BOONS TO BE CLICKED
        foreach (GameObject b in boons)
        {
            b.GetComponent<Button>().enabled = true;
        }

        //MOVE PARENT
        LeanTween.moveLocal(boonsParent, new Vector3(0, 100, 0), 1f).setEaseOutExpo();
        LeanTween.scale(boonsParent, new Vector3(0.5f, 0.5f, 1), 1f).setEaseOutExpo();

        //MOVE SWAP ICON
        LeanTween.moveLocal(swapIcon, new Vector3(0, -275, 0), 0.6f).setEaseOutExpo();

        //MOVE AND DISABLE THROW AND TAKE OPTIONS
        LeanTween.moveLocal(options[0].transform.parent.gameObject, new Vector3(0, 0, 0), 0.3f).setEaseInExpo();
        LeanTween.moveLocal(options[1].transform.parent.gameObject, new Vector3(0, 0, 0), 0.3f).setEaseInExpo();
        options[0].SetActive(false);
        options[1].SetActive(false);

        //MOVE BOON BELT
        LeanTween.moveLocal(boonBelt, new Vector3(0, -120, 0), 1).setEaseOutExpo();
        LeanTween.scale(boonBelt, new Vector3(2, 2, 1), 1).setEaseOutExpo();

        //MOVE AND ENABLE BACK OPTION
        yield return new WaitForSeconds(0.3f);
        options[2].transform.parent.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        LeanTween.moveLocal(options[2].transform.parent.gameObject, new Vector3(-190, 0, 0), 0.3f).setEaseOutExpo();
        yield return new WaitForSeconds(0.3f);
        isSelectingBelt = true;
        options[2].SetActive(true);
        EventSystem.current.SetSelectedGameObject(boons[0]);

        yield return null;
    }

    public void ReturnToOptionSelection()
    {
        if (isSelectingBelt)
        {
            StartCoroutine(ReturnToOptionSelectPhase());
        }
    }

    private IEnumerator ReturnToOptionSelectPhase()
    {
        ResetSelected();
        isSelectingOption = true;
        isSelectingBelt = false;

        //DISABLE BOON BUTTONS
        foreach (GameObject b in boons)
        {
            b.GetComponent<Button>().enabled = false;
        }

        //RESET PARENT
        LeanTween.moveLocal(boonsParent, new Vector3(0, 0, 0), 1f).setEaseOutExpo();
        LeanTween.scale(boonsParent, new Vector3(1, 1, 1), 1f).setEaseOutExpo();

        //HIDE SWAP ICON
        LeanTween.moveLocal(swapIcon, new Vector3(0, 0, 0), 1f).setEaseOutExpo();

        //MOVE AND DISABLE BACK OPTION
        LeanTween.moveLocal(options[2].transform.parent.gameObject, new Vector3(0, 0, 0), 0.3f).setEaseInExpo();
        options[2].SetActive(false);

        //RESET BOON BELT
        LeanTween.moveLocal(boonBelt, new Vector3(275, -175, 0), 1f).setEaseOutExpo();
        LeanTween.scale(boonBelt, new Vector3(1, 1, 1), 1f).setEaseOutExpo();

        //MOVE AND ENABLE THROW AND TAKE OPTIONS
        yield return new WaitForSeconds(0.3f);
        options[0].transform.parent.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        options[1].transform.parent.GetChild(1).GetComponent<Image>().color = new Color(1, 1, 1, 1);
        LeanTween.moveLocal(options[0].transform.parent.gameObject, new Vector3(-190, 0, 0), 0.3f).setEaseOutExpo();
        LeanTween.moveLocal(options[1].transform.parent.gameObject, new Vector3(190, 0, 0), 0.3f).setEaseOutExpo();
        yield return new WaitForSeconds(0.3f);
        isSelectingOption = true;
        options[0].SetActive(true);
        options[1].SetActive(true);

        yield return null;
    }

    public void SelectBoon()
    {
        StartCoroutine(SelectedBoonPhase());
    }

    private IEnumerator SelectedBoonPhase()
    {
        //GET CHOSEN BOON SLOT
        GameObject boonToFill = EventSystem.current.currentSelectedGameObject;

        isSelectingOption = false;
        isSelectingBelt = false;

        ResetSelected();

        //DISABLE BOON BUTTONS
        foreach (GameObject b in boons)
        {
            b.GetComponent<Button>().enabled = false;
        }

        //HIDE SWAP ICON
        LeanTween.moveLocal(swapIcon, new Vector3(0, 0, 0), 0.1f).setEaseOutExpo();

        //MOVE AND DISABLE BACK OPTION
        LeanTween.moveLocal(options[2].transform.parent.gameObject, new Vector3(0, 0, 0), 0.1f).setEaseInExpo();
        options[2].SetActive(false);

        //HIDE BOON MASK
        LeanTween.scale(boonMask, new Vector3(0.8f, 0.8f, 1), 0.3f).setEaseInExpo();

        //MOVE DISPLAY BOON OVER SELECTED BOON
        LeanTween.move(boonsParent, boonToFill.transform.position, 1f).setEaseOutExpo();
        LeanTween.scale(boonsParent, new Vector3(0.285f, 0.285f, 1), 1f).setEaseOutExpo();
        yield return new WaitForSeconds(1f);

        //SET BOON ON BELT SLOT
        boonSO heldBoon = boonToFill.GetComponent<boonSelectionHandler>().heldBoon;

        if (heldBoon != null)
        {
            //IF BOON SLOT IS HOLDING A BOON, THEN SUBTRACT IT FROM STATS
            gameManager.instance.UpdateStats(heldBoon, false);
        }
        //ADD TO STATS AND UI
        gameManager.instance.UpdateStats(currentBoon, true);
        boonToFill.GetComponent<boonSelectionHandler>().SetData(currentBoon);

        //RESET DISPLAY BOON
        boonsParent.transform.localPosition = new Vector2(0, 500);
        boonsParent.transform.localScale = new Vector3(1, 1, 1);

        //RESET BOON BELT
        LeanTween.moveLocal(boonBelt, new Vector3(275, -175, 0), 1f).setEaseOutExpo();
        LeanTween.scale(boonBelt, new Vector3(1, 1, 1), 1f).setEaseOutExpo();

        //FADE OUT BG AND ENABLE PLAYER
        greyBG.Play(greyExit);
        gameManager.instance.playerEnabled = true;

        spawnManager.instance.EndLevel();

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
            HandleNextBoonSelection(1);
        }

        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            HandleNextOptionSelection(-1);
            HandleNextBoonSelection(-1);
        }

        if (Input.GetAxisRaw("Vertical") > 0)
        {
            if (isSelectingBelt)
            {
                EventSystem.current.SetSelectedGameObject(options[2]);
            }
        }

        if (Input.GetAxisRaw("Vertical") < 0)
        {
            if (isSelectingBelt)
            {
                EventSystem.current.SetSelectedGameObject(boons[1]);
            }
        }
    }

    private IEnumerator SetSelectedAfterOneFrame()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HandleNextOptionSelection (int addition)
    {
        if (EventSystem.current.currentSelectedGameObject == null && isSelectingOption)
        {
            int newIndex = lastSelectedIndex + addition;
            newIndex = Mathf.Clamp(newIndex, 0, options.Length - 1);
            EventSystem.current.SetSelectedGameObject(options[newIndex]);
        }
    }

    private void HandleNextBoonSelection (int addition)
    {
        if (EventSystem.current.currentSelectedGameObject == null && isSelectingBelt)
        {
            int newIndex = lastSelectedIndex + addition;
            newIndex = Mathf.Clamp(newIndex, 0, boons.Count - 1);
            EventSystem.current.SetSelectedGameObject(boons[newIndex]);
        }
    }
}
