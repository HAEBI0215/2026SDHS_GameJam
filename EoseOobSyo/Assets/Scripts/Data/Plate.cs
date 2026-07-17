using System.Collections.Generic;
using UnityEngine;


public class Plate : ItemBase
{
    private List<ItemBase> items = new List<ItemBase>();

    public IReadOnlyList<ItemBase> Items => items;

    public bool AddItem(ItemBase item)
    {
        if(item == this)
            return false;

        items.Add(item);

        Debug.Log($"{item.ItemName} 접시에 추가");

        return true;
    }

    public void Clear()
    {
        items.Clear();
    }
}