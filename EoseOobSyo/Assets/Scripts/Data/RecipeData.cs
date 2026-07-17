using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;

    public ItemData[] ingredients;

    public float cookingTime;

    public ItemData resultItem;
}