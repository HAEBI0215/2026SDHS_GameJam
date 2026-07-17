using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DaySettlementUI : MonoBehaviour
{
    [Header("루트")]
    [SerializeField] private GameObject settlementRoot;
    [SerializeField] private CanvasGroup settlementCanvasGroup;
    [SerializeField] private RectTransform settlementWindow;

    [Header("텍스트")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text grossEarningsText;
    [SerializeField] private TMP_Text expiredPenaltyText;
    [SerializeField] private TMP_Text wrongPenaltyText;
    [SerializeField] private TMP_Text finalEarningsText;
    [SerializeField] private TMP_Text totalMoneyText;

    [Header("다음")]
    [SerializeField] private Button openShopButton;
    [SerializeField] private ShopUI shopUI;

    [Header("DOTween")]
    [SerializeField] private float openDuration = 0.28f;
    [SerializeField] private float numberDuration = 0.45f;
    [SerializeField] private float rowDelay = 0.12f;

    private GameManager gameManager;
    private Sequence settlementSequence;

    private void Awake()
    {
        if(openShopButton != null)
        {
            openShopButton.onClick.AddListener(OpenShop);
        }

        HideImmediate();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        if(gameManager == null)
        {
            Debug.LogError("DaySettlementUI에서 GameManager를 찾지 못했습니다.");
            return;
        }

        gameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        if(gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        if(openShopButton != null)
        {
            openShopButton.onClick.RemoveListener(OpenShop);
        }

        settlementSequence?.Kill();
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        if(state == GameManager.GameState.Result)
        {
            Show();
        }
    }

    public void Show()
    {
        GameProgressManager progress = GameProgressManager.Instance;

        if(progress == null)
        {
            Debug.LogError("GameProgressManager가 없습니다.");
            return;
        }

        settlementSequence?.Kill();

        settlementRoot.SetActive(true);

        if(openShopButton != null)
        {
            openShopButton.interactable = false;
        }

        dayText.text = $"DAY {progress.CurrentDay} 정산";

        SetMoneyText(grossEarningsText, 0, false);
        SetMoneyText(expiredPenaltyText, 0, true);
        SetMoneyText(wrongPenaltyText, 0, true);
        SetMoneyText(finalEarningsText, 0, false);
        SetMoneyText(totalMoneyText, 0, false);

        settlementCanvasGroup.alpha = 0f;
        settlementWindow.localScale = Vector3.one * 0.9f;

        settlementSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        settlementSequence.Append(
            settlementCanvasGroup
                .DOFade(1f, openDuration)
                .SetEase(Ease.OutQuad)
        );

        settlementSequence.Join(
            settlementWindow
                .DOScale(1f, openDuration)
                .SetEase(Ease.OutBack)
        );

        settlementSequence.AppendInterval(rowDelay);

        AppendCountAnimation(
            settlementSequence,
            grossEarningsText,
            progress.LastGrossEarnings,
            false
        );

        settlementSequence.AppendInterval(rowDelay);

        AppendCountAnimation(
            settlementSequence,
            expiredPenaltyText,
            progress.LastExpiredPenalty,
            true
        );

        settlementSequence.AppendInterval(rowDelay);

        AppendCountAnimation(
            settlementSequence,
            wrongPenaltyText,
            progress.LastWrongSubmitPenalty,
            true
        );

        settlementSequence.AppendInterval(rowDelay);

        AppendCountAnimation(
            settlementSequence,
            finalEarningsText,
            progress.LastNetEarnings,
            false
        );

        settlementSequence.AppendCallback(() =>
        {
            finalEarningsText.rectTransform
                .DOPunchScale(
                    Vector3.one * 0.15f,
                    0.25f,
                    6,
                    0.6f
                )
                .SetUpdate(true);
        });

        settlementSequence.AppendInterval(rowDelay);

        AppendCountAnimation(
            settlementSequence,
            totalMoneyText,
            progress.TotalMoney,
            false
        );

        settlementSequence.OnComplete(() =>
        {
            if(openShopButton != null)
            {
                openShopButton.interactable = true;

                openShopButton.transform
                    .DOPunchScale(
                        Vector3.one * 0.08f,
                        0.25f,
                        5,
                        0.5f
                    )
                    .SetUpdate(true);
            }
        });
    }

    private void AppendCountAnimation(
        Sequence sequence,
        TMP_Text targetText,
        int targetValue,
        bool isPenalty)
    {
        if(targetText == null)
            return;

        int displayed = 0;

        sequence.Append(
            DOTween.To(
                    () => displayed,
                    value =>
                    {
                        displayed = value;
                        SetMoneyText(
                            targetText,
                            displayed,
                            isPenalty
                        );
                    },
                    targetValue,
                    numberDuration
                )
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
        );
    }

    private void SetMoneyText(
        TMP_Text targetText,
        int value,
        bool isPenalty)
    {
        if(targetText == null)
            return;

        targetText.text =
            isPenalty
                ? $"- {value:N0}"
                : $"+ {value:N0}";
    }

    private void OpenShop()
    {
        if(shopUI == null)
        {
            Debug.LogError("ShopUI가 연결되지 않았습니다.");
            return;
        }

        if(openShopButton != null)
        {
            openShopButton.interactable = false;
        }

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        sequence.Append(
            settlementCanvasGroup
                .DOFade(0f, 0.18f)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            settlementWindow
                .DOScale(0.94f, 0.18f)
                .SetEase(Ease.InCubic)
        );

        sequence.OnComplete(() =>
        {
            settlementRoot.SetActive(false);
            shopUI.Open();
        });
    }

    private void HideImmediate()
    {
        if(settlementCanvasGroup != null)
        {
            settlementCanvasGroup.alpha = 0f;
        }

        if(settlementWindow != null)
        {
            settlementWindow.localScale = Vector3.one;
        }

        if(settlementRoot != null)
        {
            settlementRoot.SetActive(false);
        }
    }
}
