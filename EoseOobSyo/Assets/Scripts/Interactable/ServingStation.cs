using UnityEngine;

public class ServingStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerInventory inventory)
    {
        if(inventory == null)
        {
            return;
        }

        if(!GameManager.Instance.IsPlaying())
        {
            Debug.Log("현재 장사 중이 아닙니다.");
            return;
        }

        if(!inventory.HasItem())
        {
            Debug.Log("제출할 음식이 없습니다.");
            return;
        }

        ItemBase heldItem = inventory.GetItem();

        PlateItem plate = heldItem as PlateItem;

        if(plate == null)
        {
            Debug.Log("음식이 담긴 접시만 제출할 수 있습니다.");
            return;
        }

        if(!plate.HasFood())
        {
            Debug.Log("빈 접시는 제출할 수 없습니다.");
            return;
        }

        FoodItem food = plate.GetFood();

        if(food == null)
        {
            Debug.LogWarning("접시에 음식 정보가 없습니다.");
            return;
        }

        SubmitPlate(inventory, plate, food);
    }

    private void SubmitPlate(
        PlayerInventory inventory,
        PlateItem plate,
        FoodItem food
    )
    {
        if(GameManager.Instance.Order == null)
        {
            Debug.LogError("OrderManager가 GameManager에 연결되지 않았습니다.");
            return;
        }

        bool success =
            GameManager.Instance.Order.SubmitFood(food);

        if(success)
        {
            Debug.Log($"{food.ItemName} 주문 제출 성공");
        }
        else
        {
            Debug.Log($"{food.ItemName} 주문 제출 실패");
        }

        // 플레이어 손에서 접시 제거
        ItemBase submittedItem = inventory.TakeItem();

        if(submittedItem != null)
        {
            Destroy(submittedItem.gameObject);
        }
    }
}