using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("아이템 위치")]
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

    public ItemBase TakeItem()
    {
        ItemBase item = currentItem;

        currentItem = null;

        return item;
    }

    public void RemoveItem()
    {
        currentItem = null;
    }
}