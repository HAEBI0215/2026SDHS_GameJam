using UnityEngine;


public class CookingStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInventory inventory)
    {
        if(!inventory.HasItem())
        {
            Debug.Log("조리할 재료가 없습니다.");
            return;
        }

        ItemBase item = inventory.GetItem();

        if (item.ItemName == "Plate")
        {
            Debug.Log("접시는 조리할 수 없습니다");
            return;
        }

        Debug.Log($"{item.ItemName} 조리 시작");

        inventory.RemoveItem();

        Destroy(item.gameObject);
    }
}