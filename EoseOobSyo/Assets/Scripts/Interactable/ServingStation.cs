using UnityEngine;

public class ServingStation :
    MonoBehaviour,
    IInteractable
{
    public void Interact(
        PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        GameManager gameManager =
            GameManager.Instance;

        if(gameManager == null)
        {
            Debug.LogError(
                "GameManager가 없습니다."
            );

            return;
        }

        if(!gameManager.IsPlaying())
        {
            Debug.Log(
                "현재 장사 중이 아닙니다."
            );

            return;
        }

        if(!inventory.HasItem())
        {
            Debug.Log(
                "제출할 음식이 없습니다."
            );

            return;
        }

        ItemBase heldItem =
            inventory.GetItem();

        FoodItem food =
            heldItem as FoodItem;

        if(food == null)
        {
            Debug.Log(
                "조리된 음식만 제출할 수 있습니다."
            );

            return;
        }

        SubmitFood(
            inventory,
            food
        );
    }

    private void SubmitFood(
        PlayerInventory inventory,
        FoodItem food)
    {
        OrderManager orderManager =
            GameManager.Instance.Order;

        if(orderManager == null)
        {
            Debug.LogError(
                "OrderManager가 GameManager에 연결되지 않았습니다."
            );

            return;
        }

        bool success =
            orderManager.SubmitFood(
                food
            );

        if(!success)
        {
            Debug.Log(
                $"{food.ItemName}과 일치하는 주문이 없습니다."
            );

            GameManager.Instance.Score
                ?.RegisterWrongSubmit();

            // 잘못 제출한 음식은 손에 남김
            // 이후 쓰레기통에 버릴 수 있음
            return;
        }

        Debug.Log(
            $"{food.ItemName} 주문 제출 성공"
        );

        ItemBase submittedFood =
            inventory.TakeItem();

        if(submittedFood != null)
        {
            Destroy(
                submittedFood.gameObject
            );
        }
    }
}