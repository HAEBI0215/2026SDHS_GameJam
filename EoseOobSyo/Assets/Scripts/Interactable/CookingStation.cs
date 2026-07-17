using System.Collections.Generic;
using UnityEngine;

public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("가능한 레시피 목록")]
    [SerializeField]
    private List<RecipeData> recipes = new List<RecipeData>();

    [Header("실패 음식")]
    [SerializeField]
    private ItemData failedFood;

    [Header("결과 음식 생성 위치")]
    [SerializeField]
    private Transform outputPoint;

    [Header("재료 입력 유예 시간")]
    [SerializeField]
    private float inputTime = 10f;

    [Header("타이머 UI")]
    [SerializeField]
    private CookingTimerUI timerUI;

    private readonly List<ItemData> ingredients = new List<ItemData>();

    private bool isTimerRunning;
    private float currentTime;

    private GameObject currentResultObject;

    private void Update()
    {
        UpdateCookingTimer();

        // 완성 음식이 플레이어에게 집혀서
        // 조리대 위치에서 벗어난 경우 다시 조리 가능하게 처리
        if(currentResultObject != null &&
           currentResultObject.transform.parent != null)
        {
            currentResultObject = null;
        }
    }

    public void Interact(PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        // 완성된 음식이 아직 남아 있으면 새 재료 투입 방지
        if(currentResultObject != null)
        {
            Debug.Log("완성된 음식을 먼저 가져가야 합니다.");
            return;
        }

        if(!inventory.HasItem())
        {
            Debug.Log("들고 있는 아이템이 없습니다.");
            return;
        }

        ItemBase item = inventory.GetItem();

        if(item == null)
            return;

        if(item.Data == null)
        {
            Debug.LogWarning("아이템에 ItemData가 없습니다.");
            return;
        }

        if(item.Data.itemType != ItemType.Ingredient)
        {
            Debug.Log("재료만 넣을 수 있습니다.");
            return;
        }

        AddIngredient(item.Data);

        ItemBase takenItem = inventory.TakeItem();

        if(takenItem != null)
        {
            Destroy(takenItem.gameObject);
        }

        // 첫 재료를 넣은 순간에만 타이머 시작
        if(!isTimerRunning)
        {
            StartCookingTimer();
        }
    }

    private void AddIngredient(ItemData itemData)
    {
        ingredients.Add(itemData);

        Debug.Log($"{itemData.itemName} 투입");

        PrintCurrentIngredients();
    }

    private void StartCookingTimer()
    {
        isTimerRunning = true;
        currentTime = inputTime;

        if(timerUI != null)
        {
            timerUI.Show(inputTime);
        }

        Debug.Log($"조리 타이머 시작: {inputTime}초");
    }

    private void UpdateCookingTimer()
    {
        if(!isTimerRunning)
            return;

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);

        if(timerUI != null)
        {
            timerUI.UpdateTimer(currentTime, inputTime);
        }

        if(currentTime <= 0f)
        {
            FinishCooking();
        }
    }

    private void FinishCooking()
    {
        isTimerRunning = false;
        currentTime = 0f;

        if(timerUI != null)
        {
            timerUI.Hide();
        }

        RecipeData matchedRecipe = FindMatchedRecipe();

        if(matchedRecipe != null)
        {
            SpawnResult(matchedRecipe.resultItem);

            Debug.Log($"{matchedRecipe.recipeName} 완성!");
        }
        else
        {
            SpawnResult(failedFood);

            Debug.Log("일치하는 레시피가 없어 실패 음식이 생성되었습니다.");
        }

        ingredients.Clear();
    }

    private RecipeData FindMatchedRecipe()
    {
        foreach(RecipeData recipe in recipes)
        {
            if(recipe == null)
                continue;

            if(IsSameRecipe(recipe))
            {
                return recipe;
            }
        }

        return null;
    }

    private bool IsSameRecipe(RecipeData recipe)
    {
        if(recipe.ingredients == null)
            return false;

        if(recipe.ingredients.Length != ingredients.Count)
            return false;

        // 같은 재료가 여러 개 필요한 경우도 정확하게 비교하기 위해
        // 복사 리스트에서 하나씩 제거
        List<ItemData> remainingIngredients =
            new List<ItemData>(ingredients);

        foreach(ItemData requiredItem in recipe.ingredients)
        {
            if(!remainingIngredients.Remove(requiredItem))
            {
                return false;
            }
        }

        return remainingIngredients.Count == 0;
    }

    private void SpawnResult(ItemData resultItem)
    {
        if(resultItem == null)
        {
            Debug.LogError($"{name}: 결과 ItemData가 없습니다.");
            return;
        }

        if(resultItem.prefab == null)
        {
            Debug.LogError(
                $"{name}: {resultItem.itemName}의 프리팹이 없습니다."
            );
            return;
        }

        if(outputPoint == null)
        {
            Debug.LogError($"{name}: OutputPoint가 없습니다.");
            return;
        }

        currentResultObject = Instantiate(
            resultItem.prefab,
            outputPoint.position,
            Quaternion.identity
        );
    }

    private void PrintCurrentIngredients()
    {
        if(ingredients.Count == 0)
        {
            Debug.Log("현재 재료 없음");
            return;
        }

        string ingredientNames = string.Empty;

        for(int i = 0; i < ingredients.Count; i++)
        {
            ingredientNames += ingredients[i].itemName;

            if(i < ingredients.Count - 1)
            {
                ingredientNames += ", ";
            }
        }

        Debug.Log($"현재 재료: {ingredientNames}");
    }
}