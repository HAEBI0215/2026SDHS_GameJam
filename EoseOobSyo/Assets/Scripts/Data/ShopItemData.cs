using UnityEngine;

public enum ShopItemType
{
    Recipe,
    Ingredient
}

[CreateAssetMenu(
    menuName = "Cooking/Shop Item",
    fileName = "ShopItem_")]
public class ShopItemData : ScriptableObject
{
    [Header("식별 정보")]
    [SerializeField] private string itemId;

    [Header("표시 정보")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [TextArea]
    [SerializeField] private string description;

    [Header("구매 정보")]
    [SerializeField] private ShopItemType itemType;
    [SerializeField] private int price = 100;

    [Header("해금 대상")]
    [SerializeField] private RecipeData recipe;
    [SerializeField] private ItemData ingredient;

    public string ItemId =>
        string.IsNullOrWhiteSpace(itemId)
            ? name
            : itemId;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName;

    public Sprite Icon => icon;
    public string Description => description;
    public ShopItemType ItemType => itemType;
    public int Price => Mathf.Max(0, price);

    public RecipeData Recipe => recipe;
    public ItemData Ingredient => ingredient;
}
