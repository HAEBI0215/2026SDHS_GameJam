using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("주문 가능한 레시피")]
    [SerializeField]
    private List<RecipeData> recipes = new List<RecipeData>();

    private RecipeData currentOrder;

    public RecipeData CurrentOrder => currentOrder;

    public void StartOrder()
    {
        GenerateOrder();
    }

    public void StopOrder()
    {
        currentOrder = null;

        Debug.Log("주문 시스템 종료");
    }

    public void GenerateOrder()
    {
        if(!GameManager.Instance.IsPlaying())
            return;

        if(recipes == null || recipes.Count == 0)
        {
            Debug.LogWarning("주문 가능한 레시피가 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, recipes.Count);

        currentOrder = recipes[randomIndex];

        Debug.Log($"새 주문: {currentOrder.recipeName}");

        // 나중에 주문 UI 연결
        // GameManager.Instance.UI.ShowOrder(currentOrder);
    }

    public bool SubmitFood(FoodItem food)
    {
        if(!GameManager.Instance.IsPlaying())
            return false;

        if(food == null)
        {
            Debug.LogWarning("제출된 음식이 없습니다.");
            return false;
        }

        if(currentOrder == null)
        {
            Debug.LogWarning("현재 주문이 없습니다.");
            return false;
        }

        bool isCorrect = food.Data == currentOrder.resultItem;

        if(isCorrect)
        {
            Debug.Log($"주문 성공: {food.ItemName}");

            GenerateOrder();

            return true;
        }

        Debug.Log(
            $"주문 실패. 주문: {currentOrder.recipeName}, 제출: {food.ItemName}"
        );

        return false;
    }
}