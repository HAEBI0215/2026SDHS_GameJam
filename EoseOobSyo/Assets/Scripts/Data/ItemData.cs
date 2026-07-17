using UnityEngine;

public enum ItemType
{
    Ingredient,
    Food,
    Plate
}

[CreateAssetMenu(menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public ItemType itemType;

    public GameObject prefab;
}