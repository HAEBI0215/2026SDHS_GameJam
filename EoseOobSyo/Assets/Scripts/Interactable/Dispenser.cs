using UnityEngine;

public class Dispenser : MonoBehaviour, IInteractable
{
    [Header("생성할 아이템")]
    [SerializeField]
    private ItemData itemData;

    [Header("생성 위치")]
    [SerializeField]
    private Transform outputPoint;

    public void Interact(PlayerInventory inventory)
    {
        if(inventory.HasItem())
        {
            Debug.Log("손에 아이템이 있습니다");
            return;
        }
        SpawnItem();
    }

    private void SpawnItem()
    {
        Instantiate(
            itemData.prefab,
            outputPoint.position,
            Quaternion.identity
        );

        Debug.Log(
            itemData.itemName + " 생성"
        );
    }
}