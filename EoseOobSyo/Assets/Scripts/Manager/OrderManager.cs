using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("주문 생성")]
    [SerializeField]
    private float firstOrderDelay = 1f;

    [SerializeField]
    private float orderInterval = 8f;

    [SerializeField]
    private int maxActiveOrders = 4;

    [Header("주문 제한 시간")]
    [SerializeField]
    private float orderTimeLimit = 30f;

    private readonly List<ActiveOrder> activeOrders =
        new List<ActiveOrder>();

    private float orderSpawnTimer;
    private int nextOrderId;

    private bool isRunning;

    public IReadOnlyList<ActiveOrder> ActiveOrders =>
        activeOrders;

    public bool IsRunning => isRunning;

    public event Action<ActiveOrder> OnOrderAdded;
    public event Action<ActiveOrder> OnOrderCompleted;
    public event Action<ActiveOrder> OnOrderExpired;
    public event Action OnOrdersCleared;

    private void Update()
    {
        if(!isRunning)
            return;

        UpdateOrderTimes();
        UpdateOrderSpawn();
    }

    public void StartOrderSystem()
    {
        if(ShopUnlockManager.Instance == null)
        {
            Debug.LogError(
                "ShopUnlockManager가 존재하지 않습니다."
            );

            return;
        }

        // 주문 시작 직전에 해금 목록을 확실하게 다시 계산
        ShopUnlockManager.Instance.RefreshUnlockState();

        Debug.Log(
            $"주문 시작 직전 해금 레시피 수: " +
            $"{ShopUnlockManager.Instance.UnlockedRecipes.Count}"
        );

        if(!ValidateSettings())
            return;

        ClearOrders();

        isRunning = true;
        nextOrderId = 0;
        orderSpawnTimer = firstOrderDelay;

        if(firstOrderDelay <= 0f)
        {
            CreateOrder();
            orderSpawnTimer = orderInterval;
        }

        Debug.Log("주문 시스템 시작");
    }

    public void StopOrderSystem()
    {
        isRunning = false;

        Debug.Log("주문 시스템 종료");
    }

    private void UpdateOrderTimes()
    {
        float deltaTime = Time.deltaTime;

        for(int i = activeOrders.Count - 1; i >= 0; i--)
        {
            ActiveOrder order = activeOrders[i];

            order.Tick(deltaTime);

            if(!order.IsExpired)
                continue;

            activeOrders.RemoveAt(i);

            OnOrderExpired?.Invoke(order);

            string recipeName =
                order.Recipe != null
                    ? order.Recipe.recipeName
                    : "알 수 없는 주문";

            Debug.Log(
                $"주문 시간 초과: {recipeName}"
            );
        }
    }

    private void UpdateOrderSpawn()
    {
        if(activeOrders.Count >= maxActiveOrders)
            return;

        orderSpawnTimer -= Time.deltaTime;

        if(orderSpawnTimer > 0f)
            return;

        CreateOrder();

        orderSpawnTimer = orderInterval;
    }

    private bool CreateOrder()
    {
        if(activeOrders.Count >= maxActiveOrders)
            return false;

        RecipeData recipe = GetRandomRecipe();

        if(recipe == null)
        {
            Debug.LogWarning(
                "주문을 생성하지 못했습니다. " +
                "현재 해금된 레시피를 확인하세요."
            );

            return false;
        }

        ActiveOrder newOrder = new ActiveOrder(
            nextOrderId,
            recipe,
            orderTimeLimit
        );

        nextOrderId++;

        activeOrders.Add(newOrder);

        OnOrderAdded?.Invoke(newOrder);

        Debug.Log(
            $"새 주문 생성: {recipe.recipeName}"
        );

        return true;
    }

    private RecipeData GetRandomRecipe()
    {
        if(ShopUnlockManager.Instance == null)
        {
            Debug.LogError(
                "OrderManager가 ShopUnlockManager를 찾지 못했습니다."
            );

            return null;
        }

        RecipeData recipe =
            ShopUnlockManager.Instance
                .GetRandomUnlockedRecipe();

        if(recipe == null)
        {
            Debug.LogError(
                "현재 주문 가능한 해금 레시피가 없습니다."
            );

            return null;
        }

        return recipe;
    }

    public bool SubmitFood(FoodItem submittedFood)
    {
        if(!isRunning)
        {
            Debug.Log(
                "현재 주문 시스템이 작동 중이 아닙니다."
            );

            return false;
        }

        if(submittedFood == null)
        {
            Debug.LogWarning(
                "제출된 음식이 없습니다."
            );

            return false;
        }

        ItemData submittedItemData =
            submittedFood.Data;

        if(submittedItemData == null)
        {
            Debug.LogWarning(
                $"{submittedFood.name}에 ItemData가 없습니다."
            );

            return false;
        }

        int matchingOrderIndex =
            FindMatchingOrder(submittedItemData);

        if(matchingOrderIndex < 0)
        {
            Debug.Log(
                $"일치하는 주문이 없습니다: " +
                $"{submittedFood.ItemName}"
            );

            return false;
        }

        ActiveOrder completedOrder =
            activeOrders[matchingOrderIndex];

        activeOrders.RemoveAt(
            matchingOrderIndex
        );

        OnOrderCompleted?.Invoke(
            completedOrder
        );

        Debug.Log(
            $"주문 완료: " +
            $"{completedOrder.Recipe.recipeName}"
        );

        return true;
    }

    private int FindMatchingOrder(
        ItemData submittedItemData)
    {
        int matchingIndex = -1;

        float lowestRemainingTime =
            float.MaxValue;

        for(int i = 0; i < activeOrders.Count; i++)
        {
            ActiveOrder order =
                activeOrders[i];

            RecipeData recipe =
                order.Recipe;

            if(recipe == null)
                continue;

            if(recipe.resultItem != submittedItemData)
                continue;

            // 같은 음식 주문이 여러 개라면
            // 시간이 가장 적게 남은 주문부터 처리
            if(order.RemainingTime >= lowestRemainingTime)
                continue;

            matchingIndex = i;

            lowestRemainingTime =
                order.RemainingTime;
        }

        return matchingIndex;
    }

    public void ClearOrders()
    {
        activeOrders.Clear();

        OnOrdersCleared?.Invoke();
    }

    [ContextMenu("주문 강제 생성")]
    private void ForceCreateOrder()
    {
        if(!ValidateSettings())
            return;

        CreateOrder();
    }

    private bool ValidateSettings()
    {
        ShopUnlockManager unlockManager =
            ShopUnlockManager.Instance;

        if(unlockManager == null)
        {
            Debug.LogError(
                "씬에 ShopUnlockManager가 없습니다."
            );

            return false;
        }

        IReadOnlyList<RecipeData> unlockedRecipes =
            unlockManager.UnlockedRecipes;

        Debug.Log(
            $"ValidateSettings 검사 시 해금 레시피 수: " +
            $"{unlockedRecipes.Count}"
        );

        if(unlockedRecipes.Count == 0)
        {
            Debug.LogError(
                "현재 해금된 레시피가 없습니다."
            );

            return false;
        }

        for(int i = 0; i < unlockedRecipes.Count; i++)
        {
            RecipeData recipe =
                unlockedRecipes[i];

            if(recipe == null)
            {
                Debug.LogError(
                    $"해금 레시피 목록의 {i}번이 비어 있습니다."
                );

                return false;
            }

            if(recipe.resultItem == null)
            {
                Debug.LogError(
                    $"{recipe.name}의 Result Item이 없습니다."
                );

                return false;
            }
        }

        if(maxActiveOrders <= 0)
        {
            Debug.LogError(
                "Max Active Orders는 1 이상이어야 합니다."
            );

            return false;
        }

        if(orderInterval <= 0f)
        {
            Debug.LogError(
                "Order Interval은 0보다 커야 합니다."
            );

            return false;
        }

        if(orderTimeLimit <= 0f)
        {
            Debug.LogError(
                "Order Time Limit은 0보다 커야 합니다."
            );

            return false;
        }

        return true;
    }
}