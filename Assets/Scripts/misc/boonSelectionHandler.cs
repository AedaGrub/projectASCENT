using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class boonSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject scaleTarget1;
    [Range(0, 2f), SerializeField] private float scaleAmount1;
    private Vector3 startScale1;

    [SerializeField] private GameObject scaleTarget2;
    [Range(0, 2f), SerializeField] private float scaleAmount2;
    private Vector3 startScale2;

    private void Start()
    {
        startScale1 = scaleTarget1.transform.localScale;
        startScale2 = scaleTarget2.transform.localScale;
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
                endScale2 = startScale2 * scaleAmount2;
            }
            else
            {
                endScale1 = startScale1;
                endScale2 = startScale2;
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

        for (int i = 0; i < boonsSelectManager.instance.boons.Length; i++)
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
