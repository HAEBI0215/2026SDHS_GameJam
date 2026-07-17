using System.Collections.Generic;
using UnityEngine;


public class CookingStation : MonoBehaviour, IInteractable
{
    [Header("가능한 레시피")]
    [SerializeField]
    private List<RecipeData> recipes;


    [Header("실패 음식")]
    [SerializeField]
    private ItemData failedFood;


    [Header("완성 위치")]
    [SerializeField]
    private Transform outputPoint;



    [Header("재료 입력 제한 시간")]
    [SerializeField]
    private float inputTime = 10f;



    private List<ItemData> ingredients = new();


    private bool isInputting;

    private float timer;



    public void Interact(PlayerInventory inventory)
    {
        if(!inventory.HasItem())
            return;



        ItemBase item = inventory.GetItem();



        if(item.Data.itemType != ItemType.Ingredient)
        {
            Debug.Log("재료만 넣을 수 있습니다.");
            return;
        }



        AddIngredient(item.Data);



        inventory.TakeItem();

        Destroy(item.gameObject);



        // 첫 재료 투입 시 타이머 시작
        if(!isInputting)
        {
            StartInputTimer();
        }
    }




    private void AddIngredient(ItemData item)
    {
        ingredients.Add(item);


        Debug.Log(
            item.itemName + " 추가"
        );
    }



    private void StartInputTimer()
    {
        isInputting = true;

        timer = inputTime;


        Debug.Log(
            "재료 입력 시작 : 10초"
        );
    }



    private void Update()
    {
        if(!isInputting)
            return;



        timer -= Time.deltaTime;



        if(timer <= 0)
        {
            CheckRecipe();
        }
    }




    private void CheckRecipe()
    {
        isInputting = false;



        RecipeData result = FindRecipe();



        if(result != null)
        {
            Debug.Log(
                result.recipeName + " 성공"
            );


            SpawnFood(
                result.resultItem
            );
        }
        else
        {
            Debug.Log(
                "실패 음식"
            );


            SpawnFood(
                failedFood
            );
        }



        ingredients.Clear();
    }





    private RecipeData FindRecipe()
    {
        foreach(RecipeData recipe in recipes)
        {
            if(IsSameRecipe(recipe))
            {
                return recipe;
            }
        }


        return null;
    }




    private bool IsSameRecipe(RecipeData recipe)
    {
        // 재료 개수가 다르면 실패
        if(recipe.ingredients.Length != ingredients.Count)
            return false;



        foreach(ItemData need in recipe.ingredients)
        {
            if(!ingredients.Contains(need))
                return false;
        }



        return true;
    }




    private void SpawnFood(ItemData food)
    {
        Instantiate(
            food.prefab,
            outputPoint.position,
            Quaternion.identity
        );
    }
}