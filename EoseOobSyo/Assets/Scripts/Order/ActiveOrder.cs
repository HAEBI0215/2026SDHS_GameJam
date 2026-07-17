using UnityEngine;

[System.Serializable]
public class ActiveOrder
{
    [SerializeField]
    private int orderId;

    [SerializeField]
    private RecipeData recipe;

    [SerializeField]
    private float maxTime;

    [SerializeField]
    private float remainingTime;

    public int OrderId => orderId;
    public RecipeData Recipe => recipe;

    public float MaxTime => maxTime;
    public float RemainingTime => remainingTime;

    public bool IsExpired =>
        remainingTime <= 0f;

    public float RemainingRatio
    {
        get
        {
            if(maxTime <= 0f)
                return 0f;

            return Mathf.Clamp01(
                remainingTime / maxTime
            );
        }
    }

    public ActiveOrder(
        int id,
        RecipeData orderRecipe,
        float timeLimit)
    {
        orderId = id;
        recipe = orderRecipe;

        maxTime = Mathf.Max(
            0.1f,
            timeLimit
        );

        remainingTime = maxTime;
    }

    public void Tick(float deltaTime)
    {
        remainingTime = Mathf.Max(
            0f,
            remainingTime - deltaTime
        );
    }
}