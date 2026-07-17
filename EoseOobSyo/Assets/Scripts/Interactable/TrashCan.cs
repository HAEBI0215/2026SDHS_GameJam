using UnityEngine;

public class TrashCan :
    MonoBehaviour,
    IInteractable
{
    public void Interact(
        PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        if(!inventory.HasItem())
        {
            Debug.Log(
                "버릴 아이템이 없습니다."
            );

            return;
        }

        ItemBase heldItem =
            inventory.GetItem();

        if(heldItem == null)
        {
            Debug.LogWarning(
                "들고 있는 아이템 정보를 찾지 못했습니다."
            );

            return;
        }

        string itemName =
            heldItem.Data != null
                ? heldItem.Data.itemName
                : heldItem.name;

        ItemBase discardedItem =
            inventory.TakeItem();

        if(discardedItem == null)
        {
            Debug.LogWarning(
                "인벤토리에서 아이템을 꺼내지 못했습니다."
            );

            return;
        }

        Destroy(
            discardedItem.gameObject
        );

        Debug.Log(
            $"{itemName}을(를) 쓰레기통에 버렸습니다."
        );
    }
}