using UnityEngine;

public class FoodItem : ItemBase
{
    [Header("음식 품질")]
    [SerializeField]
    private FoodQuality quality =
        FoodQuality.Good;

    [SerializeField]
    private int seasoningSuccessCount;

    [SerializeField]
    private int seasoningTotalCount;

    public FoodQuality Quality =>
        quality;

    public int SeasoningSuccessCount =>
        seasoningSuccessCount;

    public int SeasoningTotalCount =>
        seasoningTotalCount;

    public override void Interact(
        PlayerInventory inventory)
    {
        if(inventory == null)
            return;

        if(inventory.HasItem())
        {
            Debug.Log(
                "이미 다른 아이템을 들고 있습니다."
            );

            return;
        }

        inventory.PickUp(this);

        Debug.Log(
            $"{ItemName}을(를) 집었습니다."
        );
    }

    public void SetQuality(
        FoodQuality newQuality,
        int successCount,
        int totalCount)
    {
        quality =
            newQuality;

        seasoningSuccessCount =
            Mathf.Max(
                0,
                successCount
            );

        seasoningTotalCount =
            Mathf.Max(
                0,
                totalCount
            );

        Debug.Log(
            $"{ItemName} 품질 설정: " +
            $"{quality} / " +
            $"{seasoningSuccessCount}/" +
            $"{seasoningTotalCount}"
        );
    }
}