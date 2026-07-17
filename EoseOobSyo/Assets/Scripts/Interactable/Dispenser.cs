using UnityEngine;

public class Dispenser : MonoBehaviour, IInteractable
{
    [Header("생성할 재료")]
    [SerializeField]
    private ItemData itemData;

    [Header("생성 위치")]
    [SerializeField]
    private Transform outputPoint;

    public void Interact(PlayerInventory inventory)
    {
        if(inventory.HasItem())
        {
            Debug.Log("이미 아이템을 들고 있습니다.");
            return;
        }

        SpawnItem(inventory);
    }

    private void SpawnItem(PlayerInventory inventory)
    {
        if(itemData == null)
        {
            Debug.LogError($"{name}: ItemData가 설정되지 않았습니다.");
            return;
        }

        if(itemData.prefab == null)
        {
            Debug.LogError($"{name}: {itemData.itemName}의 Prefab이 없습니다.");
            return;
        }

        if(outputPoint == null)
        {
            Debug.LogError($"{name}: OutputPoint가 설정되지 않았습니다.");
            return;
        }

        GameObject obj = Instantiate(
            itemData.prefab,
            outputPoint.position,
            Quaternion.identity
        );

        ItemBase item = obj.GetComponent<ItemBase>();

        if(item == null)
        {
            Debug.LogError($"{obj.name}에 ItemBase 컴포넌트가 없습니다.");
            Destroy(obj);
            return;
        }

        inventory.PickUp(item);

        Debug.Log($"{itemData.itemName} 지급");
    }
}