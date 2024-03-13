using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class boonSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("DATA")]
    public boonSO heldBoon;

    [SerializeField] private Image boonIcon;
    [SerializeField] private Image boonTier;
    [SerializeField] private Image boonFill;
    [SerializeField] private Image boonHighlightMask;
    [SerializeField] private Image boonHighlight;
    [SerializeField] private Image beltNotch;

    [Header("ADD ANIMATION")]
    [SerializeField] private GameObject beltScale;

    [Header("SELECT ANIMATION")]
    [SerializeField] private GameObject scaleTarget1;
    [Range(0, 2f), SerializeField] private float scaleAmount1;
    private Vector3 startScale1;

    [SerializeField] private GameObject scaleTarget2;

    private void Start()
    {
        startScale1 = scaleTarget1.transform.localScale;
        scaleTarget2.transform.localScale = new Vector2(0, 0);
        ScaleBelt();
        EmptyData();
    }

    public void EmptyData()
    {
        boonIcon.sprite = null;
        boonTier.sprite = null;

        boonIcon.gameObject.SetActive(false);
        boonTier.gameObject.SetActive(false);
        boonFill.gameObject.SetActive(false);
        boonHighlightMask.gameObject.SetActive(false);
    }

    public void SetData(boonSO boon)
    {
        heldBoon = boon;
        boonIcon.sprite = heldBoon.iconSprite;
        boonTier.sprite = heldBoon.tierSprite;

        boonIcon.gameObject.SetActive(true);
        boonTier.gameObject.SetActive(true);
        boonFill.gameObject.SetActive(true);
        boonHighlightMask.gameObject.SetActive(true);
    }

    private void ScaleBelt()
    {
        LeanTween.scale(beltScale, new Vector3(1.37f, 1, 1), 2f).setEaseOutExpo();
    }

    private IEnumerator SelectCard(bool startingAnim)
    {
        Vector3 endScale1;
        Vector3 endScale2;

        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;

            if (startingAnim)
            {
                endScale1 = startScale1 * scaleAmount1;
                endScale2 = new Vector3(1.2f, 1.2f, 1);
            }
            else
            {
                endScale1 = startScale1;
                endScale2 = new Vector3(0, 0, 1);
            }

            Vector3 lerpedScale1 = Vector3.Lerp(scaleTarget1.transform.localScale, endScale1, (elapsedTime / 0.1f));
            Vector3 lerpedScale2 = Vector3.Lerp(scaleTarget2.transform.localScale, endScale2, (elapsedTime / 0.1f));

            scaleTarget1.transform.localScale = lerpedScale1;
            scaleTarget2.transform.localScale = lerpedScale2;

            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boonsSelectManager.instance.isSelectingBelt)
        {
            eventData.selectedObject = gameObject;
        }
        else
        {
            eventData.selectedObject = null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        eventData.selectedObject = null;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(SelectCard(true));
        boonsSelectManager.instance.lastSelected = gameObject;

        for (int i = 0; i < boonsSelectManager.instance.boons.Count; i++)
        {
            boonsSelectManager.instance.lastSelectedIndex = i;
            return;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(SelectCard(false));
    }
}
