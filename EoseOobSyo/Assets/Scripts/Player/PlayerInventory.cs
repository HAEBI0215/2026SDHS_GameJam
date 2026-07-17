using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("아이템을 들 위치")]
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
        {
            return;
        }

        if(item == null)
        {
            return;
        }

        currentItem = item;
        currentItem.PickUp(holdPoint);
    }

    public void Drop()
    {
        if(currentItem == null)
        {
            return;
        }

        currentItem.Drop();
        currentItem = null;
    }

    public ItemBase TakeItem()
    {
        if(currentItem == null)
        {
            return null;
        }

        ItemBase item = currentItem;
        currentItem = null;

        return item;
    }

    public void RemoveItem()
    {
        currentItem = null;
    }
}