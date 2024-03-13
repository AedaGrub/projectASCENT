using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boonUIBelt : MonoBehaviour
{
    public static boonUIBelt instance;
    [SerializeField] private boonSelectionHandler boonPrefab;

    [SerializeField] private RectTransform boonContent;

    List<boonSelectionHandler> listOfUIItems = new List<boonSelectionHandler>();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DONT DESTROY ON LOAD FROM BOONSSELECTMANAGER
        }
    }

    private void Start()
    {
        InitialiseBoonBeltUI(1);
    }

    public void InitialiseBoonBeltUI(int beltSize)
    {
        for (int i = 0; i < beltSize; i++)
        {
            boonSelectionHandler boonItem = Instantiate(boonPrefab, new Vector3(1,1,1), Quaternion.identity);
            boonItem.transform.SetParent(boonContent);
            listOfUIItems.Add(boonItem);
            boonsSelectManager.instance.boons.Add(boonItem.gameObject);
        }
    }
}
