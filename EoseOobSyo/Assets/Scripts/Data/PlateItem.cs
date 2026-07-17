using UnityEngine;

public class PlateItem : ItemBase
{
    [Header("음식이 올라갈 위치")]
    [SerializeField]
    private Transform foodPoint;

    private FoodItem currentFood;

    public bool HasFood()
    {
        return currentFood != null;
    }

    public FoodItem GetFood()
    {
        return currentFood;
    }

    public bool TrySetFood(FoodItem food)
    {
        if(food == null)
        {
            Debug.LogWarning("담으려는 음식이 없습니다.");
            return false;
        }

        if(currentFood != null)
        {
            Debug.Log("접시에 이미 음식이 있습니다.");
            return false;
        }

        currentFood = food;

        Transform targetPoint = foodPoint != null
            ? foodPoint
            : transform;

        food.PutOnPlate(targetPoint);

        Debug.Log($"{food.ItemName}을 접시에 담았습니다.");

        return true;
    }

    public FoodItem RemoveFood()
    {
        FoodItem food = currentFood;
        currentFood = null;

        return food;
    }
}