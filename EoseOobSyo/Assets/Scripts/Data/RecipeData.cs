using UnityEngine;

[CreateAssetMenu(menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("레시피 정보")]
    public string recipeName;

    public ItemData[] ingredients;

    [Min(0.1f)]
    public float cookingTime = 10f;

    public ItemData resultItem;

    [Header("간 맞추기")]

    [Tooltip("조리 진행도 기준 간 맞추기 타이밍입니다.")]
    [Range(0f, 1f)]
    public float[] seasoningTimings =
    {
        0.25f,
        0.55f,
        0.85f
    };

    [Tooltip("목표 시간 전후로 성공할 수 있는 시간입니다.")]
    [Min(0.05f)]
    public float seasoningSuccessRangeSeconds = 0.6f;

    private void OnValidate()
    {
        cookingTime = Mathf.Max(0.1f, cookingTime);
        seasoningSuccessRangeSeconds =
            Mathf.Max(0.05f, seasoningSuccessRangeSeconds);

        if(seasoningTimings == null)
            return;

        for(int i = 0; i < seasoningTimings.Length; i++)
        {
            seasoningTimings[i] =
                Mathf.Clamp01(seasoningTimings[i]);
        }
    }
}