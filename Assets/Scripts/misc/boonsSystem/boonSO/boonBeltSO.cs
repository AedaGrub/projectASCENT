using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class boonBeltSO : ScriptableObject
{
    [SerializeField] private List<boonBeltItem> boonBeltItems;

    [field: SerializeField] public int size { get; private set; } = 1;

    public void Initialise()
    {
        boonBeltItems = new List<boonBeltItem>();
        for (int i = 0; i < size; i++)
        {
            boonBeltItems.Add(boonBeltItem.GetEmptyItem());
        }
    }

    public void AddItem(boonSO item, int quantity)
    {
        for (int i = 0; i < boonBeltItems.Count; i++)
        {
            if (boonBeltItems[i].isEmpty)
            {
                boonBeltItems[i] = new boonBeltItem
                {
                    item = item, 
                    quantity = quantity,
                };
            }
        }
    }

    public Dictionary<int, boonBeltItem> GetCurrentInventoryState()
    {
        Dictionary<int, boonBeltItem> returnValue = new Dictionary<int, boonBeltItem> ();

        for (int i = 0; i < boonBeltItems.Count; i++)
        {
            if (boonBeltItems[i].isEmpty)
                continue;
            returnValue[i] = boonBeltItems[i];
        }
        return returnValue;
    }
}

[Serializable]
public struct boonBeltItem
{
    public int quantity;
    public boonSO item;

    public bool isEmpty => item == null;

    public boonBeltItem ChangeQuantity(int newQuantity)
    {
        return new boonBeltItem
        {
            item = this.item,
            quantity = newQuantity,
        };
    }

    public static boonBeltItem GetEmptyItem() => new boonBeltItem
    {
        item = null,
        quantity = 0,
    };
}