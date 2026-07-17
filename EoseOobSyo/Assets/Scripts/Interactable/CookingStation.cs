using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CookingStation : MonoBehaviour, IInteractable
{
    private enum CookingState
    {
        Empty,
        IngredientInput,
        Cooking,
        ResultReady
    }

    [Header("가능한 레시피 목록")]
    [SerializeField]
    private List<RecipeData> recipes =
        new List<RecipeData>();

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

    [Header("간 맞추기 UI")]

    [Tooltip("간을 맞출 수 있는 순간에 활성화할 UI입니다.")]
    [SerializeField]
    private GameObject seasoningPrompt;

    [Tooltip("성공, 실패 결과를 표시할 텍스트입니다.")]
    [SerializeField]
    private TMP_Text seasoningResultText;

    [SerializeField]
    private float seasoningResultDuration = 0.7f;

    private readonly List<ItemData> ingredients =
        new List<ItemData>();

    private CookingState currentState =
        CookingState.Empty;

    private float currentInputTime;

    private RecipeData currentRecipe;
    private float currentCookingTime;

    private float[] currentSeasoningTimings =
        Array.Empty<float>();

    private int currentSeasoningIndex;
    private int seasoningSuccessCount;

    private GameObject currentResultObject;

    private Coroutine seasoningResultCoroutine;

    private void Awake()
    {
        HideSeasoningUI();

        if(timerUI != null)
        {
            timerUI.Hide();
        }
    }

    private void Update()
    {
        switch(currentState)
        {
            case CookingState.IngredientInput:
                UpdateIngredientInputTimer();
                break;

            case CookingState.Cooking:
                UpdateCooking();
                break;

            case CookingState.ResultReady:
                UpdateResultState();
                break;
        }
    }

    public void Interact(PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        switch(currentState)
        {
            case CookingState.Empty:
            case CookingState.IngredientInput:
                TryAddIngredient(inventory);
                break;

            case CookingState.Cooking:
                TrySeasoning();
                break;

            case CookingState.ResultReady:
                Debug.Log(
                    "완성된 음식을 접시에 담아 가져가야 합니다."
                );
                break;
        }
    }

    private void TryAddIngredient(
        PlayerInventory inventory)
    {
        if(!inventory.HasItem())
        {
            Debug.Log(
                "조리대에 넣을 재료를 들고 있지 않습니다."
            );

            return;
        }

        ItemBase item = inventory.GetItem();

        if(item == null)
            return;

        if(item.Data == null)
        {
            Debug.LogWarning(
                "아이템에 ItemData가 없습니다."
            );

            return;
        }

        if(item.Data.itemType != ItemType.Ingredient)
        {
            Debug.Log(
                "조리대에는 재료만 넣을 수 있습니다."
            );

            return;
        }

        AddIngredient(item.Data);

        ItemBase takenItem =
            inventory.TakeItem();

        if(takenItem != null)
        {
            Destroy(takenItem.gameObject);
        }

        if(currentState == CookingState.Empty)
        {
            StartIngredientInput();
        }
    }

    private void AddIngredient(ItemData itemData)
    {
        ingredients.Add(itemData);

        Debug.Log(
            $"{itemData.itemName} 투입"
        );

        PrintCurrentIngredients();
    }

    private void StartIngredientInput()
    {
        currentState =
            CookingState.IngredientInput;

        currentInputTime =
            Mathf.Max(0.1f, inputTime);

        if(timerUI != null)
        {
            timerUI.Show(currentInputTime);

            timerUI.UpdateTimer(
                currentInputTime,
                inputTime
            );
        }

        Debug.Log(
            $"재료 입력 유예시간 시작: {inputTime}초"
        );
    }

    private void UpdateIngredientInputTimer()
    {
        currentInputTime -= Time.deltaTime;

        currentInputTime =
            Mathf.Max(currentInputTime, 0f);

        if(timerUI != null)
        {
            timerUI.UpdateTimer(
                currentInputTime,
                inputTime
            );
        }

        if(currentInputTime > 0f)
            return;

        FinishIngredientInput();
    }

    private void FinishIngredientInput()
    {
        currentInputTime = 0f;

        RecipeData matchedRecipe =
            FindMatchedRecipe();

        if(matchedRecipe == null)
        {
            Debug.Log(
                "일치하는 레시피가 없어 실패 음식이 생성됩니다."
            );

            ingredients.Clear();

            if(timerUI != null)
            {
                timerUI.Hide();
            }

            bool spawned =
                SpawnResult(
                    failedFood,
                    FoodQuality.Poor,
                    0,
                    0
                );

            if(spawned)
            {
                currentState =
                    CookingState.ResultReady;
            }
            else
            {
                ResetStation();
            }

            return;
        }

        BeginCooking(matchedRecipe);
    }

    private void BeginCooking(
        RecipeData recipe)
    {
        currentRecipe = recipe;

        currentState =
            CookingState.Cooking;

        currentCookingTime = 0f;

        currentSeasoningIndex = 0;
        seasoningSuccessCount = 0;

        PrepareSeasoningTimings(recipe);

        if(timerUI != null)
        {
            timerUI.Show(
                recipe.cookingTime
            );

            timerUI.UpdateTimer(
                recipe.cookingTime,
                recipe.cookingTime
            );
        }

        SetSeasoningPrompt(false);

        Debug.Log(
            $"{recipe.recipeName} 조리 시작 / " +
            $"조리시간: {recipe.cookingTime}초 / " +
            $"간 맞추기 횟수: " +
            $"{currentSeasoningTimings.Length}회"
        );
    }

    private void PrepareSeasoningTimings(
        RecipeData recipe)
    {
        if(recipe.seasoningTimings == null ||
           recipe.seasoningTimings.Length == 0)
        {
            currentSeasoningTimings =
                Array.Empty<float>();

            return;
        }

        currentSeasoningTimings =
            new float[
                recipe.seasoningTimings.Length
            ];

        Array.Copy(
            recipe.seasoningTimings,
            currentSeasoningTimings,
            recipe.seasoningTimings.Length
        );

        Array.Sort(
            currentSeasoningTimings
        );
    }

    private void UpdateCooking()
    {
        if(currentRecipe == null)
        {
            Debug.LogError(
                "조리 중인 레시피가 없습니다."
            );

            ResetStation();
            return;
        }

        currentCookingTime +=
            Time.deltaTime;

        currentCookingTime =
            Mathf.Min(
                currentCookingTime,
                currentRecipe.cookingTime
            );

        float remainingTime =
            Mathf.Max(
                0f,
                currentRecipe.cookingTime -
                currentCookingTime
            );

        if(timerUI != null)
        {
            timerUI.UpdateTimer(
                remainingTime,
                currentRecipe.cookingTime
            );
        }

        UpdateMissedSeasoningWindows();
        UpdateSeasoningPrompt();

        if(currentCookingTime <
           currentRecipe.cookingTime)
        {
            return;
        }

        FinishCooking();
    }

    private void UpdateMissedSeasoningWindows()
    {
        if(currentRecipe == null)
            return;

        while(currentSeasoningIndex <
              currentSeasoningTimings.Length)
        {
            float targetTime =
                GetCurrentSeasoningTargetTime();

            float lateLimit =
                targetTime +
                currentRecipe
                    .seasoningSuccessRangeSeconds;

            if(currentCookingTime <= lateLimit)
                break;

            Debug.Log(
                $"간 맞추기 실패 " +
                $"({currentSeasoningIndex + 1}/" +
                $"{currentSeasoningTimings.Length})"
            );

            ShowSeasoningResult(
                "간 맞추기 실패!"
            );

            currentSeasoningIndex++;
        }
    }

    private void UpdateSeasoningPrompt()
    {
        if(currentRecipe == null ||
           currentSeasoningIndex >=
           currentSeasoningTimings.Length)
        {
            SetSeasoningPrompt(false);
            return;
        }

        float targetTime =
            GetCurrentSeasoningTargetTime();

        float difference =
            Mathf.Abs(
                currentCookingTime -
                targetTime
            );

        bool isInsideWindow =
            difference <=
            currentRecipe
                .seasoningSuccessRangeSeconds;

        SetSeasoningPrompt(
            isInsideWindow
        );
    }

    private void TrySeasoning()
    {
        if(currentRecipe == null)
            return;

        UpdateMissedSeasoningWindows();

        if(currentSeasoningIndex >=
           currentSeasoningTimings.Length)
        {
            Debug.Log(
                "이번 요리의 간 맞추기는 모두 끝났습니다."
            );

            return;
        }

        float targetTime =
            GetCurrentSeasoningTargetTime();

        float earlyLimit =
            targetTime -
            currentRecipe
                .seasoningSuccessRangeSeconds;

        float lateLimit =
            targetTime +
            currentRecipe
                .seasoningSuccessRangeSeconds;

        if(currentCookingTime < earlyLimit)
        {
            Debug.Log(
                "아직 간을 맞출 타이밍이 아닙니다."
            );

            ShowSeasoningResult(
                "아직!"
            );

            return;
        }

        if(currentCookingTime > lateLimit)
        {
            UpdateMissedSeasoningWindows();
            return;
        }

        seasoningSuccessCount++;

        Debug.Log(
            $"간 맞추기 성공! " +
            $"({seasoningSuccessCount}/" +
            $"{currentSeasoningTimings.Length})"
        );

        ShowSeasoningResult(
            "손맛 성공!"
        );

        currentSeasoningIndex++;

        SetSeasoningPrompt(false);
    }

    private float GetCurrentSeasoningTargetTime()
    {
        if(currentRecipe == null ||
           currentSeasoningIndex >=
           currentSeasoningTimings.Length)
        {
            return float.MaxValue;
        }

        float normalizedTiming =
            Mathf.Clamp01(
                currentSeasoningTimings[
                    currentSeasoningIndex
                ]
            );

        return currentRecipe.cookingTime *
               normalizedTiming;
    }

    private void FinishCooking()
    {
        SetSeasoningPrompt(false);

        while(currentSeasoningIndex <
              currentSeasoningTimings.Length)
        {
            Debug.Log(
                $"간 맞추기 실패 " +
                $"({currentSeasoningIndex + 1}/" +
                $"{currentSeasoningTimings.Length})"
            );

            currentSeasoningIndex++;
        }

        if(timerUI != null)
        {
            timerUI.Hide();
        }

        FoodQuality quality =
            CalculateFoodQuality();

        string recipeName =
            currentRecipe.recipeName;

        ItemData resultItem =
            currentRecipe.resultItem;

        int totalSeasoningCount =
            currentSeasoningTimings.Length;

        ingredients.Clear();

        bool spawned =
            SpawnResult(
                resultItem,
                quality,
                seasoningSuccessCount,
                totalSeasoningCount
            );

        if(spawned)
        {
            currentState =
                CookingState.ResultReady;

            Debug.Log(
                $"{recipeName} 완성! / " +
                $"품질: {quality} / " +
                $"간 맞추기: " +
                $"{seasoningSuccessCount}/" +
                $"{totalSeasoningCount}"
            );
        }
        else
        {
            ResetStation();
        }

        currentRecipe = null;
        currentCookingTime = 0f;
    }

    private FoodQuality CalculateFoodQuality()
    {
        int totalCount =
            currentSeasoningTimings.Length;

        if(totalCount <= 0)
        {
            return FoodQuality.Good;
        }

        if(seasoningSuccessCount >= totalCount)
        {
            return FoodQuality.Perfect;
        }

        if(seasoningSuccessCount >=
           Mathf.Max(1, totalCount - 1))
        {
            return FoodQuality.Good;
        }

        return FoodQuality.Poor;
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

    private bool IsSameRecipe(
        RecipeData recipe)
    {
        if(recipe.ingredients == null)
            return false;

        if(recipe.ingredients.Length !=
           ingredients.Count)
        {
            return false;
        }

        List<ItemData> remainingIngredients =
            new List<ItemData>(ingredients);

        foreach(
            ItemData requiredItem
            in recipe.ingredients)
        {
            if(!remainingIngredients.Remove(
                   requiredItem))
            {
                return false;
            }
        }

        return remainingIngredients.Count == 0;
    }

    private bool SpawnResult(
        ItemData resultItem,
        FoodQuality quality,
        int successCount,
        int totalCount)
    {
        if(resultItem == null)
        {
            Debug.LogError(
                $"{name}: 결과 ItemData가 없습니다."
            );

            return false;
        }

        if(resultItem.prefab == null)
        {
            Debug.LogError(
                $"{name}: {resultItem.itemName}의 " +
                $"프리팹이 없습니다."
            );

            return false;
        }

        if(outputPoint == null)
        {
            Debug.LogError(
                $"{name}: OutputPoint가 없습니다."
            );

            return false;
        }

        currentResultObject =
            Instantiate(
                resultItem.prefab,
                outputPoint.position,
                Quaternion.identity
            );

        FoodItem foodItem =
            currentResultObject
                .GetComponent<FoodItem>();

        if(foodItem != null)
        {
            foodItem.SetQuality(
                quality,
                successCount,
                totalCount
            );
        }
        else
        {
            Debug.LogWarning(
                $"{resultItem.itemName} 프리팹에 " +
                $"FoodItem이 없습니다."
            );
        }

        return true;
    }

    private void UpdateResultState()
    {
        // 접시에 담기면서 부모가 생긴 경우
        if(currentResultObject != null &&
           currentResultObject.transform.parent != null)
        {
            currentResultObject = null;
            ResetStation();
            return;
        }

        // 외부에서 음식이 파괴된 경우
        if(currentResultObject == null)
        {
            ResetStation();
        }
    }

    private void ResetStation()
    {
        currentState =
            CookingState.Empty;

        ingredients.Clear();

        currentInputTime = 0f;

        currentRecipe = null;
        currentCookingTime = 0f;

        currentSeasoningTimings =
            Array.Empty<float>();

        currentSeasoningIndex = 0;
        seasoningSuccessCount = 0;

        currentResultObject = null;

        if(timerUI != null)
        {
            timerUI.Hide();
        }

        HideSeasoningUI();
    }

    private void SetSeasoningPrompt(
        bool isActive)
    {
        if(seasoningPrompt != null)
        {
            seasoningPrompt.SetActive(
                isActive
            );
        }
    }

    private void ShowSeasoningResult(
        string message)
    {
        if(seasoningResultText == null)
            return;

        seasoningResultText.text =
            message;

        seasoningResultText.gameObject
            .SetActive(true);

        if(seasoningResultCoroutine != null)
        {
            StopCoroutine(
                seasoningResultCoroutine
            );
        }

        seasoningResultCoroutine =
            StartCoroutine(
                HideSeasoningResultRoutine()
            );
    }

    private IEnumerator
        HideSeasoningResultRoutine()
    {
        yield return new WaitForSeconds(
            seasoningResultDuration
        );

        if(seasoningResultText != null)
        {
            seasoningResultText.gameObject
                .SetActive(false);
        }

        seasoningResultCoroutine = null;
    }

    private void HideSeasoningUI()
    {
        SetSeasoningPrompt(false);

        if(seasoningResultCoroutine != null)
        {
            StopCoroutine(
                seasoningResultCoroutine
            );

            seasoningResultCoroutine = null;
        }

        if(seasoningResultText != null)
        {
            seasoningResultText.gameObject
                .SetActive(false);
        }
    }

    private void PrintCurrentIngredients()
    {
        if(ingredients.Count == 0)
        {
            Debug.Log("현재 재료 없음");
            return;
        }

        string ingredientNames =
            string.Empty;

        for(int i = 0;
            i < ingredients.Count;
            i++)
        {
            ingredientNames +=
                ingredients[i].itemName;

            if(i < ingredients.Count - 1)
            {
                ingredientNames += ", ";
            }
        }

        Debug.Log(
            $"현재 재료: {ingredientNames}"
        );
    }
}