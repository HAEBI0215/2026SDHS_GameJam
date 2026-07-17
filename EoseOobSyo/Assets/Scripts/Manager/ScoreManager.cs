using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("주문 성공 수익")]
    [SerializeField]
    private int baseOrderScore = 100;

    [SerializeField]
    private int maxTimeBonus = 100;

    [Header("음식 품질 배율")]
    [SerializeField]
    private float poorQualityMultiplier = 0.7f;

    [SerializeField]
    private float goodQualityMultiplier = 1f;

    [SerializeField]
    private float perfectQualityMultiplier = 1.5f;

    [Header("감점")]
    [SerializeField]
    private int expiredOrderPenalty = 50;

    [Header("주문 성공 사운드")]
    [SerializeField]
    private int moneySoundRepeatCount = 4;

    [SerializeField]
    private float moneySoundInterval = 0.07f;

    [SerializeField]
    [Range(0f, 1f)]
    private float moneySoundVolume = 0.9f;

    [SerializeField]
    private int wrongSubmitPenalty = 25;

    [Header("돈 설정")]
    [SerializeField]
    private bool allowNegativeScore = false;

    private OrderManager orderManager;

    private int currentScore;

    private int totalEarnedMoney;
    private int totalExpiredPenalty;
    private int totalWrongSubmitPenalty;

    private int completedOrderCount;
    private int expiredOrderCount;
    private int wrongSubmitCount;

    public int CurrentScore =>
        currentScore;

    public int TotalEarnedMoney =>
        totalEarnedMoney;

    public int TotalExpiredPenalty =>
        totalExpiredPenalty;

    public int TotalWrongSubmitPenalty =>
        totalWrongSubmitPenalty;

    public int TotalPenalty =>
        totalExpiredPenalty +
        totalWrongSubmitPenalty;

    public int CompletedOrderCount =>
        completedOrderCount;

    public int ExpiredOrderCount =>
        expiredOrderCount;

    public int WrongSubmitCount =>
        wrongSubmitCount;

    public event Action<int>
        OnScoreChanged;

    public event Action<int, int, int>
        OnOrderCountChanged;

    public event Action<ActiveOrder, int>
        OnOrderScoreAdded;

    public void Initialize(
        OrderManager targetOrderManager)
    {
        UnsubscribeOrderEvents();

        orderManager =
            targetOrderManager;

        SubscribeOrderEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeOrderEvents();
    }

    private void SubscribeOrderEvents()
    {
        if(orderManager == null)
        {
            Debug.LogWarning(
                "ScoreManager에 OrderManager가 연결되지 않았습니다."
            );

            return;
        }

        orderManager.OnOrderCompletedWithFood +=
            HandleOrderCompleted;

        orderManager.OnOrderExpired +=
            HandleOrderExpired;
    }

    private void UnsubscribeOrderEvents()
    {
        if(orderManager == null)
            return;

        orderManager.OnOrderCompletedWithFood -=
            HandleOrderCompleted;

        orderManager.OnOrderExpired -=
            HandleOrderExpired;
    }

    public void ResetScore()
    {
        currentScore = 0;

        totalEarnedMoney = 0;
        totalExpiredPenalty = 0;
        totalWrongSubmitPenalty = 0;

        completedOrderCount = 0;
        expiredOrderCount = 0;
        wrongSubmitCount = 0;

        OnScoreChanged?.Invoke(
            currentScore
        );

        NotifyOrderCountChanged();

        Debug.Log("오늘 매출 초기화");
    }

    private void HandleOrderCompleted(
    ActiveOrder completedOrder,
    FoodItem submittedFood)
{
    if(completedOrder == null)
        return;

    completedOrderCount++;

    int earnedMoney =
        CalculateOrderScore(
            completedOrder,
            submittedFood
        );

    totalEarnedMoney +=
        earnedMoney;

    RecalculateCurrentScore();
    NotifyOrderCountChanged();

    OnOrderScoreAdded?.Invoke(
        completedOrder,
        earnedMoney
    );

    // 주문 성공 돈 사운드
    if(SoundManager.Instance != null)
    {
        SoundManager.Instance.PlayMoneySound(
            moneySoundRepeatCount,
            moneySoundInterval,
            moneySoundVolume
        );
    }

    FoodQuality quality =
        submittedFood != null
            ? submittedFood.Quality
            : FoodQuality.Good;

    Debug.Log(
        $"주문 성공 +{earnedMoney} / " +
        $"품질: {quality} / " +
        $"총 수익: {totalEarnedMoney} / " +
        $"현재 정산 금액: {currentScore}"
    );
}

    private int CalculateOrderScore(
        ActiveOrder order,
        FoodItem submittedFood)
    {
        float remainingRatio =
            Mathf.Clamp01(
                order.RemainingRatio
            );

        int timeBonus =
            Mathf.RoundToInt(
                maxTimeBonus *
                remainingRatio
            );

        int originalMoney =
            baseOrderScore +
            timeBonus;

        FoodQuality quality =
            submittedFood != null
                ? submittedFood.Quality
                : FoodQuality.Good;

        float qualityMultiplier =
            GetQualityMultiplier(
                quality
            );

        return Mathf.RoundToInt(
            originalMoney *
            qualityMultiplier
        );
    }

    private float GetQualityMultiplier(
        FoodQuality quality)
    {
        switch(quality)
        {
            case FoodQuality.Poor:
                return poorQualityMultiplier;

            case FoodQuality.Perfect:
                return perfectQualityMultiplier;

            case FoodQuality.Good:
            default:
                return goodQualityMultiplier;
        }
    }

    private void HandleOrderExpired(
        ActiveOrder expiredOrder)
    {
        expiredOrderCount++;

        totalExpiredPenalty +=
            expiredOrderPenalty;

        RecalculateCurrentScore();
        NotifyOrderCountChanged();

        string recipeName =
            expiredOrder != null &&
            expiredOrder.Recipe != null
                ? expiredOrder.Recipe.recipeName
                : "알 수 없는 주문";

        Debug.Log(
            $"{recipeName} 주문 시간 초과 " +
            $"-{expiredOrderPenalty} / " +
            $"시간 초과 감점 누계: " +
            $"{totalExpiredPenalty} / " +
            $"현재 정산 금액: {currentScore}"
        );
    }

    public void RegisterWrongSubmit()
    {
        wrongSubmitCount++;

        totalWrongSubmitPenalty +=
            wrongSubmitPenalty;

        RecalculateCurrentScore();
        NotifyOrderCountChanged();

        Debug.Log(
            $"잘못된 음식 제출 " +
            $"-{wrongSubmitPenalty} / " +
            $"오제출 감점 누계: " +
            $"{totalWrongSubmitPenalty} / " +
            $"현재 정산 금액: {currentScore}"
        );
    }

    private void RecalculateCurrentScore()
    {
        int calculated =
            totalEarnedMoney -
            totalExpiredPenalty -
            totalWrongSubmitPenalty;

        currentScore =
            allowNegativeScore
                ? calculated
                : Mathf.Max(
                    0,
                    calculated
                );

        OnScoreChanged?.Invoke(
            currentScore
        );
    }

    private void NotifyOrderCountChanged()
    {
        OnOrderCountChanged?.Invoke(
            completedOrderCount,
            expiredOrderCount,
            wrongSubmitCount
        );
    }

    [ContextMenu("테스트 수익 +100")]
    private void TestAddMoney()
    {
        totalEarnedMoney += 100;

        RecalculateCurrentScore();
    }

    [ContextMenu("테스트 잘못된 제출")]
    private void TestWrongSubmit()
    {
        RegisterWrongSubmit();
    }
}