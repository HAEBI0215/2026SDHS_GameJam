using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private Transform holdPoint;

    private ItemBase currentItem;

    public bool HasItem()
    {
        return currentItem != null;
    }

    public ItemBase GetItem()
    {
        return currentItem;
    }

    public void PickUp(ItemBase item)
    {
        if(currentItem != null)
            return;

        currentItem = item;

        item.PickUp(holdPoint);
    }

    public void Drop()
    {
        if(currentItem == null)
            return;

        currentItem.Drop();

        currentItem = null;
    }

    public void RemoveItem()
    {
        currentItem = null;
    }
}