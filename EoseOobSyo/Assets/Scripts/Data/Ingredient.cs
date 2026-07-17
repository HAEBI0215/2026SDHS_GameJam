using UnityEngine;

public class Ingredient : ItemBase
{
    public enum IngredientType
    {
        Rice,
        Meat,
        Vegetable,
        Gochujang,
        Egg,
        Doenjang,
        Kim,
        Noodle,
        Chicken,
        Oil,
        SoySauce,
        Water
    }

    [SerializeField]
    private IngredientType type;

    public IngredientType Type => type;
}