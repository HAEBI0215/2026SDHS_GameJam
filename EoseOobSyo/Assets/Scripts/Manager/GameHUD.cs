using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("돈 UI")]
    [SerializeField]
    private TMP_Text moneyText;

    [SerializeField]
    private RectTransform moneyPanel;

    [SerializeField]
    private int targetMoney = 1000;

    [Header("시간 UI")]
    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private Image timeFillImage;

    [SerializeField]
    private RectTransform timePanel;

    [Header("운영 상태 UI")]
    [SerializeField]
    private TMP_Text shopStateText;

    [Header("설정 UI")]
    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button settingsCloseButton;

    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private RectTransform settingsPanelRect;

    [SerializeField]
    private CanvasGroup settingsCanvasGroup;

    [Header("DOTween 연출")]
    [SerializeField]
    private float moneyPunchPower = 0.1f;

    [SerializeField]
    private float moneyPunchDuration = 0.2f;

    [SerializeField]
    private float settingsAnimationDuration = 0.2f;

    private GameManager gameManager;
    private ScoreManager scoreManager;
    private TimeManager timeManager;

    private bool isBound;
    private bool isSettingsOpen;

    private float previousTimeScale = 1f;

    private void Start()
    {
        BindManagers();
        InitializeSettingsUI();
        RefreshAll();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    private void OnDisable()
    {
        UnbindManagers();
        KillTweens();

        if(isSettingsOpen)
        {
            Time.timeScale = previousTimeScale;
            isSettingsOpen = false;
        }
    }

    private void OnDestroy()
    {
        RemoveButtonEvents();
    }

    private void BindManagers()
    {
        if(isBound)
            return;

        gameManager = GameManager.Instance;

        if(gameManager == null)
        {
            Debug.LogError(
                "GameHUD에서 GameManager를 찾을 수 없습니다."
            );

            return;
        }

        scoreManager = gameManager.Score;
        timeManager = gameManager.Time;

        if(scoreManager != null)
        {
            scoreManager.OnScoreChanged +=
                HandleMoneyChanged;
        }

        if(timeManager != null)
        {
            timeManager.OnTimeChanged +=
                HandleTimeChanged;
        }

        gameManager.OnGameStateChanged +=
            HandleGameStateChanged;

        isBound = true;
    }

    private void UnbindManagers()
    {
        if(!isBound)
            return;

        if(scoreManager != null)
        {
            scoreManager.OnScoreChanged -=
                HandleMoneyChanged;
        }

        if(timeManager != null)
        {
            timeManager.OnTimeChanged -=
                HandleTimeChanged;
        }

        if(gameManager != null)
        {
            gameManager.OnGameStateChanged -=
                HandleGameStateChanged;
        }

        isBound = false;
    }

    private void RefreshAll()
    {
        int currentMoney =
            scoreManager != null
                ? scoreManager.CurrentScore
                : 0;

        float remainingTime =
            timeManager != null
                ? timeManager.RemainingTime
                : 0f;

        HandleMoneyChanged(
            currentMoney,
            false
        );

        HandleTimeChanged(
            remainingTime
        );

        if(gameManager != null)
        {
            HandleGameStateChanged(
                gameManager.CurrentState
            );
        }
    }

    private void HandleMoneyChanged(
        int money)
    {
        HandleMoneyChanged(
            money,
            true
        );
    }

    private void HandleMoneyChanged(
        int money,
        bool playAnimation)
    {
        if(moneyText != null)
        {
            moneyText.text =
                $"{money:N0} / {targetMoney:N0}";
        }

        if(playAnimation)
        {
            PlayMoneyAnimation();
        }
    }

    private void HandleTimeChanged(
        float remainingTime)
    {
        if(timeManager == null)
            return;

        float totalTime =
            timeManager.GameDuration;

        if(timeText != null)
        {
            timeText.text =
                $"{FormatTime(remainingTime)} / " +
                $"{FormatTime(totalTime)}";
        }

        if(timeFillImage != null)
        {
            float ratio =
                totalTime > 0f
                    ? remainingTime / totalTime
                    : 0f;

            timeFillImage.fillAmount =
                Mathf.Clamp01(ratio);
        }
    }

    private void HandleGameStateChanged(
        GameManager.GameState state)
    {
        if(shopStateText == null)
            return;

        switch(state)
        {
            case GameManager.GameState.Ready:
                shopStateText.text = "READY";
                break;

            case GameManager.GameState.Cooking:
                shopStateText.text = "OPEN";
                break;

            case GameManager.GameState.Result:
                shopStateText.text = "CLOSED";

                if(isSettingsOpen)
                {
                    CloseSettings();
                }

                break;
        }
    }

    private string FormatTime(
        float time)
    {
        int totalSeconds =
            Mathf.CeilToInt(
                Mathf.Max(0f, time)
            );

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;

        return $"{minutes:00}:{seconds:00}";
    }

    private void PlayMoneyAnimation()
    {
        if(moneyPanel == null)
            return;

        moneyPanel.DOKill();
        moneyPanel.localScale = Vector3.one;

        moneyPanel.DOPunchScale(
            Vector3.one * moneyPunchPower,
            moneyPunchDuration,
            4,
            0.5f
        );
    }

    private void InitializeSettingsUI()
    {
        if(settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                ToggleSettings
            );

            settingsButton.onClick.AddListener(
                ToggleSettings
            );
        }

        if(settingsCloseButton != null)
        {
            settingsCloseButton.onClick.RemoveListener(
                CloseSettings
            );

            settingsCloseButton.onClick.AddListener(
                CloseSettings
            );
        }

        if(settingsPanel == null)
            return;

        if(settingsPanelRect == null)
        {
            settingsPanelRect =
                settingsPanel.GetComponent<RectTransform>();
        }

        if(settingsCanvasGroup == null)
        {
            settingsCanvasGroup =
                settingsPanel.GetComponent<CanvasGroup>();
        }

        if(settingsCanvasGroup == null)
        {
            settingsCanvasGroup =
                settingsPanel.AddComponent<CanvasGroup>();
        }

        CloseSettingsImmediate();
    }

    public void ToggleSettings()
    {
        if(settingsPanel == null)
            return;

        if(isSettingsOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }

    public void OpenSettings()
    {
        if(settingsPanel == null ||
           isSettingsOpen)
        {
            return;
        }

        isSettingsOpen = true;

        previousTimeScale =
            Time.timeScale;

        Time.timeScale = 0f;

        settingsPanel.SetActive(true);

        if(settingsPanelRect != null)
        {
            settingsPanelRect.DOKill();
            settingsPanelRect.localScale =
                Vector3.one * 0.9f;

            settingsPanelRect
                .DOScale(
                    Vector3.one,
                    settingsAnimationDuration
                )
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        if(settingsCanvasGroup != null)
        {
            settingsCanvasGroup.DOKill();
            settingsCanvasGroup.alpha = 0f;

            settingsCanvasGroup
                .DOFade(
                    1f,
                    settingsAnimationDuration
                )
                .SetUpdate(true);
        }
    }

    public void CloseSettings()
    {
        if(settingsPanel == null ||
           !isSettingsOpen)
        {
            return;
        }

        isSettingsOpen = false;

        Time.timeScale =
            previousTimeScale;

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(settingsPanelRect != null)
        {
            settingsPanelRect.DOKill();

            sequence.Join(
                settingsPanelRect.DOScale(
                    Vector3.one * 0.9f,
                    settingsAnimationDuration
                )
            );
        }

        if(settingsCanvasGroup != null)
        {
            settingsCanvasGroup.DOKill();

            sequence.Join(
                settingsCanvasGroup.DOFade(
                    0f,
                    settingsAnimationDuration
                )
            );
        }

        sequence.OnComplete(() =>
        {
            if(settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        });
    }

    private void CloseSettingsImmediate()
    {
        isSettingsOpen = false;

        if(settingsPanelRect != null)
        {
            settingsPanelRect.localScale =
                Vector3.one;
        }

        if(settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 0f;
        }

        settingsPanel.SetActive(false);
    }

    private void RemoveButtonEvents()
    {
        if(settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(
                ToggleSettings
            );
        }

        if(settingsCloseButton != null)
        {
            settingsCloseButton.onClick.RemoveListener(
                CloseSettings
            );
        }
    }

    private void KillTweens()
    {
        moneyPanel?.DOKill();
        timePanel?.DOKill();
        settingsPanelRect?.DOKill();
        settingsCanvasGroup?.DOKill();
    }
}