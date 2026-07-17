using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopUnlockManager : MonoBehaviour
{
    public static ShopUnlockManager Instance
    {
        get;
        private set;
    }

    [Serializable]
    private class IngredientUnlockEntry
    {
        [Header("재료")]
        public ItemData ingredient;

        [Header("상점 상품")]
        public ShopItemData shopItem;

        [Header("씬의 디스펜서")]
        public GameObject dispenserObject;

        [Header("처음부터 해금")]
        public bool unlockedByDefault;
    }

    [Header("재료와 디스펜서")]
    [SerializeField]
    private IngredientUnlockEntry[] ingredientEntries;

    [Header("전체 레시피")]
    [SerializeField]
    private RecipeData[] allRecipes;

    private readonly List<RecipeData>
        unlockedRecipes =
            new List<RecipeData>();

    public IReadOnlyList<RecipeData>
        UnlockedRecipes =>
            unlockedRecipes;

    private void Awake()
    {
        if(Instance != null &&
           Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RefreshUnlockState();
    }

    private void Start()
    {
        GameProgressManager progress =
            GameProgressManager.Instance;

        if(progress != null)
        {
            progress.OnItemPurchased +=
                HandleItemPurchased;
        }
    }

    private void OnDestroy()
    {
        GameProgressManager progress =
            GameProgressManager.Instance;

        if(progress != null)
        {
            progress.OnItemPurchased -=
                HandleItemPurchased;
        }

        if(Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleItemPurchased(
        ShopItemData purchasedItem)
    {
        RefreshUnlockState();
    }

    public void RefreshUnlockState()
    {
        RefreshDispensers();
        RefreshRecipes();
    }

    private void RefreshDispensers()
    {
        if(ingredientEntries == null)
            return;

        foreach(
            IngredientUnlockEntry entry
            in ingredientEntries)
        {
            if(entry == null)
                continue;

            bool unlocked =
                IsIngredientUnlocked(
                    entry.ingredient
                );

            if(entry.dispenserObject != null)
            {
                entry.dispenserObject.SetActive(
                    unlocked
                );
            }
        }
    }

    private void RefreshRecipes()
    {
        unlockedRecipes.Clear();

        if(allRecipes == null)
            return;

        foreach(RecipeData recipe in allRecipes)
        {
            if(IsRecipeUnlocked(recipe))
            {
                unlockedRecipes.Add(recipe);
            }
        }

        Debug.Log(
            $"현재 주문 가능한 레시피: " +
            $"{unlockedRecipes.Count}개"
        );
    }

    public bool IsIngredientUnlocked(
        ItemData ingredient)
    {
        if(ingredient == null ||
           ingredientEntries == null)
        {
            return false;
        }

        foreach(
            IngredientUnlockEntry entry
            in ingredientEntries)
        {
            if(entry == null ||
               entry.ingredient != ingredient)
            {
                continue;
            }

            if(entry.unlockedByDefault)
            {
                return true;
            }

            if(entry.shopItem == null)
            {
                return false;
            }

            GameProgressManager progress =
                GameProgressManager.Instance;

            return progress != null &&
                   progress.IsPurchased(
                       entry.shopItem
                   );
        }

        Debug.LogWarning(
            $"{ingredient.name} 재료가 " +
            $"ShopUnlockManager에 등록되지 않았습니다."
        );

        return false;
    }

    public bool IsRecipeUnlocked(
        RecipeData recipe)
    {
        if(recipe == null ||
           recipe.ingredients == null)
        {
            return false;
        }

        foreach(
            ItemData ingredient
            in recipe.ingredients)
        {
            if(!IsIngredientUnlocked(
                   ingredient))
            {
                return false;
            }
        }

        return true;
    }

    public RecipeData GetRandomUnlockedRecipe()
    {
        if(unlockedRecipes.Count == 0)
        {
            Debug.LogWarning(
                "현재 해금된 레시피가 없습니다."
            );

            return null;
        }

        int randomIndex =
            UnityEngine.Random.Range(
                0,
                unlockedRecipes.Count
            );

        return unlockedRecipes[randomIndex];
    }
}