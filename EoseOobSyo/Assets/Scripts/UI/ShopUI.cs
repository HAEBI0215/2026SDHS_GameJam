using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("루트")]
    [SerializeField] private GameObject shopRoot;
    [SerializeField] private CanvasGroup shopCanvasGroup;
    [SerializeField] private RectTransform shopWindow;

    [Header("돈")]
    [SerializeField] private TMP_Text currentMoneyText;
    [SerializeField] private TMP_Text spentPopupText;

    [Header("스크롤 목록")]
    [SerializeField] private RectTransform content;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private ShopItemData[] shopItems;

    [Header("다음 날")]
    [SerializeField] private Button nextDayButton;

    [Header("DOTween")]
    [SerializeField] private float openDuration = 0.28f;
    [SerializeField] private float moneyCountDuration = 0.35f;

    private readonly List<ShopItemUI> spawnedItems =
        new List<ShopItemUI>();

    private int displayedMoney;

    private void Awake()
    {
        if(nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(StartNextDay);
        }

        HideImmediate();
    }

    private void OnDestroy()
    {
        if(nextDayButton != null)
        {
            nextDayButton.onClick.RemoveListener(StartNextDay);
        }
    }

    public void Open()
    {
        GameProgressManager progress =
            GameProgressManager.Instance;

        if(progress == null)
        {
            Debug.LogError("GameProgressManager가 없습니다.");
            return;
        }

        shopRoot.SetActive(true);

        BuildItemList();

        displayedMoney = progress.TotalMoney;
        RefreshMoneyImmediate();

        if(spentPopupText != null)
        {
            spentPopupText.alpha = 0f;
        }

        shopCanvasGroup.alpha = 0f;
        shopWindow.localScale = Vector3.one * 0.9f;

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        sequence.Append(
            shopCanvasGroup
                .DOFade(1f, openDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            shopWindow
                .DOScale(1f, openDuration)
                .SetEase(Ease.OutBack)
        );

        if(nextDayButton != null)
        {
            nextDayButton.interactable = true;
        }
    }

    private void BuildItemList()
    {
        ClearItemList();

        if(content == null ||
           itemPrefab == null ||
           shopItems == null)
        {
            return;
        }

        foreach(ShopItemData itemData in shopItems)
        {
            if(itemData == null)
                continue;

            ShopItemUI itemUI =
                Instantiate(itemPrefab, content, false);

            itemUI.Setup(itemData, this);
            spawnedItems.Add(itemUI);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void ClearItemList()
    {
        foreach(ShopItemUI item in spawnedItems)
        {
            if(item != null)
            {
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }

    public void TryPurchase(ShopItemUI itemUI)
    {
        if(itemUI == null ||
           itemUI.ItemData == null)
        {
            return;
        }

        GameProgressManager progress =
            GameProgressManager.Instance;

        if(progress == null)
            return;

        int beforeMoney = progress.TotalMoney;

        bool success =
            progress.TryPurchase(itemUI.ItemData);

        if(!success)
        {
            itemUI.PlayPurchaseFail();
            return;
        }

        int afterMoney = progress.TotalMoney;

        itemUI.PlayPurchaseSuccess();

        AnimateMoneyDecrease(
            beforeMoney,
            afterMoney,
            itemUI.ItemData.Price
        );

        RefreshAllItemStates();
    }

    private void AnimateMoneyDecrease(
        int beforeMoney,
        int afterMoney,
        int spentAmount)
    {
        displayedMoney = beforeMoney;

        DOTween.To(
                () => displayedMoney,
                value =>
                {
                    displayedMoney = value;

                    if(currentMoneyText != null)
                    {
                        currentMoneyText.text =
                            $"{displayedMoney:N0}";
                    }
                },
                afterMoney,
                moneyCountDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

        if(currentMoneyText != null)
        {
            currentMoneyText.rectTransform
                .DOPunchScale(
                    Vector3.one * 0.12f,
                    0.3f,
                    7,
                    0.55f
                )
                .SetUpdate(true);
        }

        PlaySpentPopup(spentAmount);
    }

    private void PlaySpentPopup(int amount)
    {
        if(spentPopupText == null)
            return;

        spentPopupText.DOKill();

        RectTransform rect =
            spentPopupText.rectTransform;

        rect.DOKill();

        Vector2 startPosition =
            rect.anchoredPosition;

        spentPopupText.text =
            $"- {amount:N0}";

        spentPopupText.alpha = 1f;

        rect.anchoredPosition =
            startPosition;

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        sequence.Join(
            rect.DOAnchorPosY(
                startPosition.y + 32f,
                0.55f
            )
            .SetEase(Ease.OutCubic)
        );

        sequence.Join(
            spentPopupText
                .DOFade(0f, 0.55f)
                .SetEase(Ease.InQuad)
        );

        sequence.OnComplete(() =>
        {
            rect.anchoredPosition =
                startPosition;
        });
    }

    private void RefreshMoneyImmediate()
    {
        if(currentMoneyText != null)
        {
            currentMoneyText.text =
                $"{displayedMoney:N0}";
        }
    }

    private void RefreshAllItemStates()
    {
        foreach(ShopItemUI item in spawnedItems)
        {
            item?.RefreshState();
        }
    }

    private void StartNextDay()
    {
        if(nextDayButton != null)
        {
            nextDayButton.interactable = false;
        }

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        sequence.Append(
            shopCanvasGroup
                .DOFade(0f, 0.2f)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            shopWindow
                .DOScale(0.92f, 0.2f)
                .SetEase(Ease.InCubic)
        );

        sequence.OnComplete(() =>
        {
            Time.timeScale = 1f;

            GameProgressManager.Instance?
                .AdvanceDay();

            int currentSceneIndex =
                UnityEngine.SceneManagement
                    .SceneManager
                    .GetActiveScene()
                    .buildIndex;

            UnityEngine.SceneManagement
                .SceneManager
                .LoadScene(
                    currentSceneIndex,
                    UnityEngine.SceneManagement
                        .LoadSceneMode.Single
                );
        });
    }

    private void HideImmediate()
    {
        if(shopCanvasGroup != null)
        {
            shopCanvasGroup.alpha = 0f;
        }

        if(shopWindow != null)
        {
            shopWindow.localScale = Vector3.one;
        }

        if(shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
    }
}
