using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayResultUI : MonoBehaviour
{
    [Header("정산 패널")]
    [SerializeField]
    private GameObject resultPanel;

    [Header("정산 텍스트")]
    [SerializeField]
    private TMP_Text dayText;

    [SerializeField]
    private TMP_Text earnedMoneyText;

    [SerializeField]
    private TMP_Text totalMoneyText;

    [SerializeField]
    private TMP_Text completedOrderText;

    [SerializeField]
    private TMP_Text expiredOrderText;

    [SerializeField]
    private TMP_Text wrongSubmitText;

    [Header("버튼")]
    [SerializeField]
    private Button nextDayButton;

    private GameManager gameManager;

    private void Awake()
    {
        if(resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if(nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(
                HandleNextDayButton
            );
        }
    }

    private void Start()
    {
        BindGameManager();
    }

    private void OnDestroy()
    {
        UnbindGameManager();

        if(nextDayButton != null)
        {
            nextDayButton.onClick.RemoveListener(
                HandleNextDayButton
            );
        }
    }

    private void BindGameManager()
    {
        gameManager = GameManager.Instance;

        if(gameManager == null)
        {
            Debug.LogError(
                "DayResultUI에서 GameManager를 찾지 못했습니다."
            );

            return;
        }

        gameManager.OnGameStateChanged +=
            HandleGameStateChanged;

        // UI가 늦게 생성된 경우 현재 상태를 바로 확인
        HandleGameStateChanged(
            gameManager.CurrentState
        );
    }

    private void UnbindGameManager()
    {
        if(gameManager == null)
            return;

        gameManager.OnGameStateChanged -=
            HandleGameStateChanged;

        gameManager = null;
    }

    private void HandleGameStateChanged(
        GameManager.GameState state)
    {
        if(state == GameManager.GameState.Result)
        {
            ShowResult();
        }
        else
        {
            HideResult();
        }
    }

    private void ShowResult()
    {
        GameProgressManager progress =
            GameProgressManager.Instance;

        if(progress == null)
        {
            Debug.LogError(
                "GameProgressManager가 없습니다."
            );

            return;
        }

        if(dayText != null)
        {
            dayText.text =
                $"DAY {progress.CurrentDay} 영업 종료";
        }

        // 감점을 모두 계산한 오늘의 최종 획득 금액
        if(earnedMoneyText != null)
        {
            earnedMoneyText.text =
                $"+ {progress.LastNetEarnings:N0}";
        }

        if(totalMoneyText != null)
        {
            totalMoneyText.text =
                $"{progress.TotalMoney:N0}";
        }

        if(completedOrderText != null)
        {
            completedOrderText.text =
                progress.LastCompletedOrders
                    .ToString();
        }

        if(expiredOrderText != null)
        {
            expiredOrderText.text =
                progress.LastExpiredOrders
                    .ToString();
        }

        if(wrongSubmitText != null)
        {
            wrongSubmitText.text =
                progress.LastWrongSubmitCount
                    .ToString();
        }

        if(nextDayButton != null)
        {
            nextDayButton.interactable = true;
        }

        if(resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    private void HideResult()
    {
        if(resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void HandleNextDayButton()
    {
        if(nextDayButton != null)
        {
            nextDayButton.interactable = false;
        }

        if(GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager가 없습니다."
            );

            return;
        }

        GameManager.Instance.StartNextDay();
    }
}