using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("진행 정보")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int totalMoney;

    [Header("구매 정보")]
    [SerializeField] private List<string> purchasedItemIds = new List<string>();

    private bool isCurrentDaySettled;

    public int CurrentDay => currentDay;
    public int TotalMoney => totalMoney;

    public int LastGrossEarnings { get; private set; }
    public int LastExpiredPenalty { get; private set; }
    public int LastWrongSubmitPenalty { get; private set; }
    public int LastNetEarnings { get; private set; }

    public int LastCompletedOrders { get; private set; }
    public int LastExpiredOrders { get; private set; }
    public int LastWrongSubmitCount { get; private set; }

    public event Action<int> OnTotalMoneyChanged;
    public event Action<ShopItemData> OnItemPurchased;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BeginDay()
    {
        isCurrentDaySettled = false;

        LastGrossEarnings = 0;
        LastExpiredPenalty = 0;
        LastWrongSubmitPenalty = 0;
        LastNetEarnings = 0;

        LastCompletedOrders = 0;
        LastExpiredOrders = 0;
        LastWrongSubmitCount = 0;

        Debug.Log($"DAY {currentDay} 시작");
    }

    public void SettleDay(
        int grossEarnings,
        int expiredPenalty,
        int wrongSubmitPenalty,
        int netEarnings,
        int completedOrders,
        int expiredOrders,
        int wrongSubmitCount)
    {
        if(isCurrentDaySettled)
        {
            Debug.LogWarning("이미 정산이 끝난 날짜입니다.");
            return;
        }

        isCurrentDaySettled = true;

        LastGrossEarnings = Mathf.Max(0, grossEarnings);
        LastExpiredPenalty = Mathf.Max(0, expiredPenalty);
        LastWrongSubmitPenalty = Mathf.Max(0, wrongSubmitPenalty);
        LastNetEarnings = Mathf.Max(0, netEarnings);

        LastCompletedOrders = Mathf.Max(0, completedOrders);
        LastExpiredOrders = Mathf.Max(0, expiredOrders);
        LastWrongSubmitCount = Mathf.Max(0, wrongSubmitCount);

        totalMoney += LastNetEarnings;
        OnTotalMoneyChanged?.Invoke(totalMoney);

        Debug.Log(
            $"DAY {currentDay} 정산 / " +
            $"획득 {LastGrossEarnings} - " +
            $"시간초과 {LastExpiredPenalty} - " +
            $"오제출 {LastWrongSubmitPenalty} = " +
            $"최종 {LastNetEarnings} / " +
            $"보유 돈 {totalMoney}"
        );
    }

    public void AdvanceDay()
    {
        currentDay++;
        BeginDay();
    }

    public bool IsPurchased(ShopItemData item)
    {
        if(item == null)
            return false;

        return purchasedItemIds.Contains(item.ItemId);
    }

    public bool TryPurchase(ShopItemData item)
    {
        if(item == null)
            return false;

        if(IsPurchased(item))
            return false;

        if(item.Price < 0 || totalMoney < item.Price)
            return false;

        totalMoney -= item.Price;
        purchasedItemIds.Add(item.ItemId);

        OnTotalMoneyChanged?.Invoke(totalMoney);
        OnItemPurchased?.Invoke(item);

        Debug.Log(
            $"{item.DisplayName} 구매 / -{item.Price} / " +
            $"남은 돈: {totalMoney}"
        );

        return true;
    }

    public bool IsItemUnlocked(string itemId)
    {
        if(string.IsNullOrWhiteSpace(itemId))
            return false;

        return purchasedItemIds.Contains(itemId);
    }

    public void AddMoney(int amount)
    {
        totalMoney = Mathf.Max(0, totalMoney + amount);

        OnTotalMoneyChanged?.Invoke(totalMoney);

        Debug.Log(
            $"[CHEAT] 돈 {amount:+#,##0;-#,##0;0} / " +
            $"현재 보유 돈: {totalMoney:N0}"
        );
    }
}
