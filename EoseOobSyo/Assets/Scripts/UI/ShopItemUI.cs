using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("표시")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;

    [Header("상태")]
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject purchasedLabel;
    [SerializeField] private RectTransform cardRect;
    [SerializeField] private CanvasGroup canvasGroup;

    private ShopItemData itemData;
    private ShopUI owner;

    public ShopItemData ItemData => itemData;

    private void Awake()
    {
        if(buyButton != null)
        {
            buyButton.onClick.AddListener(HandleBuy);
        }

        if(cardRect == null)
        {
            cardRect = transform as RectTransform;
        }

        if(canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void OnDestroy()
    {
        if(buyButton != null)
        {
            buyButton.onClick.RemoveListener(HandleBuy);
        }
    }

    public void Setup(ShopItemData data, ShopUI shopOwner)
    {
        itemData = data;
        owner = shopOwner;

        if(itemData == null)
            return;

        if(iconImage != null)
        {
            iconImage.sprite = itemData.Icon;
            iconImage.enabled = itemData.Icon != null;
            iconImage.preserveAspect = true;
        }

        if(nameText != null)
            nameText.text = itemData.DisplayName;

        if(typeText != null)
        {
            typeText.text =
                itemData.ItemType == ShopItemType.Recipe
                    ? "레시피"
                    : "재료";
        }

        if(descriptionText != null)
            descriptionText.text = itemData.Description;

        if(priceText != null)
            priceText.text = $"{itemData.Price:N0}";

        RefreshState();

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup
                .DOFade(1f, 0.2f)
                .SetUpdate(true);
        }

        if(cardRect != null)
        {
            cardRect.localScale = Vector3.one * 0.94f;
            cardRect
                .DOScale(1f, 0.22f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    public void RefreshState()
    {
        bool purchased =
            GameProgressManager.Instance != null &&
            GameProgressManager.Instance.IsPurchased(itemData);

        if(buyButton != null)
            buyButton.interactable = !purchased;

        if(purchasedLabel != null)
            purchasedLabel.SetActive(purchased);
    }

    public void PlayPurchaseSuccess()
    {
        RefreshState();

        if(cardRect != null)
        {
            cardRect.DOKill();

            cardRect
                .DOPunchScale(
                    Vector3.one * 0.1f,
                    0.28f,
                    7,
                    0.55f
                )
                .SetUpdate(true);
        }
    }

    public void PlayPurchaseFail()
    {
        if(cardRect != null)
        {
            cardRect.DOKill();

            cardRect
                .DOShakeAnchorPos(
                    0.28f,
                    new Vector2(14f, 0f),
                    18,
                    90f,
                    false,
                    true
                )
                .SetUpdate(true);
        }

        if(priceText != null)
        {
            priceText.rectTransform.DOKill();

            priceText.rectTransform
                .DOPunchScale(
                    Vector3.one * 0.14f,
                    0.25f,
                    6,
                    0.5f
                )
                .SetUpdate(true);
        }
    }

    private void HandleBuy()
    {
        owner?.TryPurchase(this);
    }
}
