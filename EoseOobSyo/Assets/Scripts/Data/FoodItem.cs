using UnityEngine;

public class FoodItem : ItemBase
{
    private bool isOnPlate;

    public bool IsOnPlate => isOnPlate;

    public override void Interact(PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        if(isOnPlate)
            return;

        if(!inventory.HasItem())
        {
            Debug.Log("음식이 뜨거워서 맨손으로 집을 수 없습니다.");
            return;
        }

        ItemBase heldItem = inventory.GetItem();

        if(heldItem is not PlateItem plate)
        {
            Debug.Log("완성 음식을 담으려면 접시가 필요합니다.");
            return;
        }

        if(plate.HasFood())
        {
            Debug.Log("접시에 이미 음식이 있습니다.");
            return;
        }

        plate.TrySetFood(this);
    }

    public void PutOnPlate(Transform foodPoint)
    {
        isOnPlate = true;

        transform.SetParent(foodPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach(Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}