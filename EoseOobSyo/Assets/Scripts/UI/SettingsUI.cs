using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    private enum TabType
    {
        Menu,
        Control,
        Sound
    }

    [Header("루트")]
    [SerializeField]
    private GameObject settingsRoot;

    [SerializeField]
    private CanvasGroup dimmedBackground;

    [SerializeField]
    private RectTransform windowRoot;

    [Header("탭 버튼")]
    [SerializeField]
    private Button menuTabButton;

    [SerializeField]
    private Button controlTabButton;

    [SerializeField]
    private Button soundTabButton;

    [Header("패널")]
    [SerializeField]
    private CanvasGroup menuPanel;

    [SerializeField]
    private CanvasGroup controlPanel;

    [SerializeField]
    private CanvasGroup soundPanel;

    [Header("메뉴 패널 버튼")]
    [SerializeField]
    private Button restartDayButton;

    [SerializeField]
    private Button returnTitleButton;

    [SerializeField]
    private Button continueButton;

    [Header("씬 설정")]
    [SerializeField]
    private string titleSceneName = "Title";

    [SerializeField]
    private float sceneChangeDuration = 0.2f;

    [Header("DOTween 설정")]
    [SerializeField]
    private float openDuration = 0.25f;

    [SerializeField]
    private float closeDuration = 0.2f;

    [SerializeField]
    private float tabSwitchDuration = 0.18f;

    [SerializeField]
    private float panelSlideDistance = 30f;

    [SerializeField]
    private float activeTabScale = 1.08f;

    [SerializeField]
    private float inactiveTabScale = 1f;

    private bool isOpen;
    private bool isTransitioning;

    private float previousTimeScale = 1f;

    private TabType currentTab = TabType.Menu;

    private Vector2 windowDefaultPosition;
    private Vector2 menuPanelDefaultPosition;
    private Vector2 controlPanelDefaultPosition;
    private Vector2 soundPanelDefaultPosition;

    private void Awake()
    {
        CacheDefaultPositions();
        BindButtons();
        CloseImmediate();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }

    private void OnDestroy()
    {
        UnbindButtons();
        KillTweens();

        if(isOpen)
        {
            Time.timeScale = previousTimeScale;
        }
    }

    private void CacheDefaultPositions()
    {
        if(windowRoot != null)
        {
            windowDefaultPosition = windowRoot.anchoredPosition;
        }

        menuPanelDefaultPosition =
            GetPanelPosition(menuPanel);

        controlPanelDefaultPosition =
            GetPanelPosition(controlPanel);

        soundPanelDefaultPosition =
            GetPanelPosition(soundPanel);
    }

    private Vector2 GetPanelPosition(CanvasGroup panel)
    {
        if(panel == null)
            return Vector2.zero;

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        return rect != null
            ? rect.anchoredPosition
            : Vector2.zero;
    }

    private void BindButtons()
    {
        if(menuTabButton != null)
        {
            menuTabButton.onClick.AddListener(OpenMenuTab);
        }

        if(controlTabButton != null)
        {
            controlTabButton.onClick.AddListener(OpenControlTab);
        }

        if(soundTabButton != null)
        {
            soundTabButton.onClick.AddListener(OpenSoundTab);
        }

        if(restartDayButton != null)
        {
            restartDayButton.onClick.AddListener(RestartCurrentDay);
        }

        if(returnTitleButton != null)
        {
            returnTitleButton.onClick.AddListener(ReturnToTitle);
        }

        if(continueButton != null)
        {
            continueButton.onClick.AddListener(CloseSettings);
        }
    }

    private void UnbindButtons()
    {
        if(menuTabButton != null)
        {
            menuTabButton.onClick.RemoveListener(OpenMenuTab);
        }

        if(controlTabButton != null)
        {
            controlTabButton.onClick.RemoveListener(OpenControlTab);
        }

        if(soundTabButton != null)
        {
            soundTabButton.onClick.RemoveListener(OpenSoundTab);
        }

        if(restartDayButton != null)
        {
            restartDayButton.onClick.RemoveListener(RestartCurrentDay);
        }

        if(returnTitleButton != null)
        {
            returnTitleButton.onClick.RemoveListener(ReturnToTitle);
        }

        if(continueButton != null)
        {
            continueButton.onClick.RemoveListener(CloseSettings);
        }
    }

    public void OpenSettings()
    {
        if(isOpen || isTransitioning)
            return;

        isOpen = true;
        isTransitioning = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if(settingsRoot != null)
        {
            settingsRoot.SetActive(true);
        }

        // ESC로 열 때마다 무조건 메뉴 탭부터 표시
        ShowOnlyPanelImmediate(TabType.Menu);

        if(dimmedBackground != null)
        {
            dimmedBackground.DOKill();
            dimmedBackground.alpha = 0f;
        }

        if(windowRoot != null)
        {
            windowRoot.DOKill();
            windowRoot.localScale = Vector3.one * 0.92f;
            windowRoot.anchoredPosition =
                windowDefaultPosition + new Vector2(0f, 18f);
        }

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(dimmedBackground != null)
        {
            sequence.Join(
                dimmedBackground
                    .DOFade(1f, openDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        if(windowRoot != null)
        {
            sequence.Join(
                windowRoot
                    .DOScale(Vector3.one, openDuration)
                    .SetEase(Ease.OutBack)
            );

            sequence.Join(
                windowRoot
                    .DOAnchorPos(
                        windowDefaultPosition,
                        openDuration
                    )
                    .SetEase(Ease.OutCubic)
            );
        }

        sequence.OnComplete(() =>
        {
            isTransitioning = false;
        });
    }

    public void CloseSettings()
    {
        if(!isOpen || isTransitioning)
            return;

        isTransitioning = true;

        SetAllButtonsInteractable(false);

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(dimmedBackground != null)
        {
            dimmedBackground.DOKill();

            sequence.Join(
                dimmedBackground
                    .DOFade(0f, closeDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

        if(windowRoot != null)
        {
            windowRoot.DOKill();

            sequence.Join(
                windowRoot
                    .DOScale(
                        Vector3.one * 0.92f,
                        closeDuration
                    )
                    .SetEase(Ease.InCubic)
            );

            sequence.Join(
                windowRoot
                    .DOAnchorPos(
                        windowDefaultPosition +
                        new Vector2(0f, 18f),
                        closeDuration
                    )
                    .SetEase(Ease.InCubic)
            );
        }

        sequence.OnComplete(() =>
        {
            isOpen = false;
            isTransitioning = false;

            if(settingsRoot != null)
            {
                settingsRoot.SetActive(false);
            }

            ResetWindowTransform();
            Time.timeScale = previousTimeScale;
        });
    }

    public void OpenMenuTab()
    {
        SwitchTab(TabType.Menu);
    }

    public void OpenControlTab()
    {
        SwitchTab(TabType.Control);
    }

    public void OpenSoundTab()
    {
        SwitchTab(TabType.Sound);
    }

    private void SwitchTab(TabType targetTab)
    {
        if(!isOpen || isTransitioning)
            return;

        if(currentTab == targetTab)
            return;

        CanvasGroup currentPanel =
            GetPanel(currentTab);

        CanvasGroup targetPanel =
            GetPanel(targetTab);

        if(currentPanel == null ||
           targetPanel == null)
        {
            return;
        }

        RectTransform currentRect =
            currentPanel.GetComponent<RectTransform>();

        RectTransform targetRect =
            targetPanel.GetComponent<RectTransform>();

        Vector2 currentBasePosition =
            GetPanelDefaultPosition(currentTab);

        Vector2 targetBasePosition =
            GetPanelDefaultPosition(targetTab);

        float direction =
            GetTabOrder(targetTab) >
            GetTabOrder(currentTab)
                ? -1f
                : 1f;

        isTransitioning = true;

        currentPanel.interactable = false;
        currentPanel.blocksRaycasts = false;

        currentPanel.DOKill();
        targetPanel.DOKill();

        if(currentRect != null)
        {
            currentRect.DOKill();
        }

        if(targetRect != null)
        {
            targetRect.DOKill();
        }

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        // 현재 패널 퇴장
        sequence.Append(
            currentPanel
                .DOFade(0f, tabSwitchDuration)
                .SetEase(Ease.OutQuad)
        );

        if(currentRect != null)
        {
            sequence.Join(
                currentRect
                    .DOAnchorPos(
                        currentBasePosition +
                        new Vector2(
                            panelSlideDistance *
                            direction,
                            0f
                        ),
                        tabSwitchDuration
                    )
                    .SetEase(Ease.OutQuad)
            );
        }

        // 현재 패널 비활성화 후 다음 패널 활성화
        sequence.AppendCallback(() =>
        {
            currentPanel.gameObject.SetActive(false);

            PreparePanelForShow(
                targetPanel,
                targetRect,
                targetBasePosition -
                new Vector2(
                    panelSlideDistance *
                    direction,
                    0f
                )
            );

            currentTab = targetTab;
            UpdateTabVisuals(false);
        });

        // 다음 패널 등장
        sequence.Append(
            targetPanel
                .DOFade(1f, tabSwitchDuration)
                .SetEase(Ease.OutQuad)
        );

        if(targetRect != null)
        {
            sequence.Join(
                targetRect
                    .DOAnchorPos(
                        targetBasePosition,
                        tabSwitchDuration
                    )
                    .SetEase(Ease.OutBack)
            );
        }

        sequence.OnComplete(() =>
        {
            targetPanel.interactable = true;
            targetPanel.blocksRaycasts = true;

            isTransitioning = false;
        });
    }

    public void RestartCurrentDay()
    {
        if(!isOpen || isTransitioning)
            return;

        isTransitioning = true;

        SetAllButtonsInteractable(false);

        PlaySceneExitAnimation(() =>
        {
            Time.timeScale = 1f;

            string currentSceneName =
                UnityEngine.SceneManagement
                    .SceneManager
                    .GetActiveScene()
                    .name;

            UnityEngine.SceneManagement
                .SceneManager
                .LoadScene(
                    currentSceneName,
                    UnityEngine.SceneManagement
                        .LoadSceneMode.Single
                );
        });
    }

    public void ReturnToTitle()
    {
        if(!isOpen || isTransitioning)
            return;

        isTransitioning = true;
        SetAllButtonsInteractable(false);

        PlaySceneExitAnimation(() =>
        {
            Time.timeScale = 1f;

            UnityEngine.SceneManagement
                .SceneManager
                .LoadScene(titleSceneName);
        });
    }

    private void PlaySceneExitAnimation(
        System.Action onComplete)
    {
        CanvasGroup currentPanel =
            GetPanel(currentTab);

        RectTransform currentPanelRect =
            currentPanel != null
                ? currentPanel.GetComponent<RectTransform>()
                : null;

        Sequence sequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(currentPanel != null)
        {
            currentPanel.DOKill();

            sequence.Join(
                currentPanel
                    .DOFade(0f, sceneChangeDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        if(currentPanelRect != null)
        {
            currentPanelRect.DOKill();

            Vector2 basePosition =
                GetPanelDefaultPosition(currentTab);

            sequence.Join(
                currentPanelRect
                    .DOAnchorPos(
                        basePosition +
                        new Vector2(0f, 20f),
                        sceneChangeDuration
                    )
                    .SetEase(Ease.InCubic)
            );
        }

        if(windowRoot != null)
        {
            windowRoot.DOKill();

            sequence.Join(
                windowRoot
                    .DOScale(
                        Vector3.one * 0.9f,
                        sceneChangeDuration
                    )
                    .SetEase(Ease.InBack)
            );
        }

        if(dimmedBackground != null)
        {
            dimmedBackground.DOKill();

            sequence.Join(
                dimmedBackground
                    .DOFade(0f, sceneChangeDuration)
                    .SetEase(Ease.InQuad)
            );
        }

        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    private void ShowOnlyPanelImmediate(TabType tab)
    {
        HidePanelImmediate(
            menuPanel,
            menuPanelDefaultPosition
        );

        HidePanelImmediate(
            controlPanel,
            controlPanelDefaultPosition
        );

        HidePanelImmediate(
            soundPanel,
            soundPanelDefaultPosition
        );

        CanvasGroup targetPanel =
            GetPanel(tab);

        if(targetPanel != null)
        {
            targetPanel.gameObject.SetActive(true);
            targetPanel.alpha = 1f;
            targetPanel.interactable = true;
            targetPanel.blocksRaycasts = true;

            RectTransform rect =
                targetPanel.GetComponent<RectTransform>();

            if(rect != null)
            {
                rect.anchoredPosition =
                    GetPanelDefaultPosition(tab);
            }
        }

        currentTab = tab;
        UpdateTabVisuals(true);
    }

    private void PreparePanelForShow(
        CanvasGroup panel,
        RectTransform rect,
        Vector2 startPosition)
    {
        panel.gameObject.SetActive(true);
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        if(rect != null)
        {
            rect.anchoredPosition = startPosition;
        }
    }

    private void HidePanelImmediate(
        CanvasGroup panel,
        Vector2 defaultPosition)
    {
        if(panel == null)
            return;

        panel.DOKill();

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        if(rect != null)
        {
            rect.DOKill();
            rect.anchoredPosition = defaultPosition;
        }

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    private void CloseImmediate()
    {
        isOpen = false;
        isTransitioning = false;

        ShowOnlyPanelImmediate(TabType.Menu);

        if(dimmedBackground != null)
        {
            dimmedBackground.alpha = 0f;
        }

        ResetWindowTransform();

        if(settingsRoot != null)
        {
            settingsRoot.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void ResetWindowTransform()
    {
        if(windowRoot == null)
            return;

        windowRoot.localScale = Vector3.one;
        windowRoot.anchoredPosition =
            windowDefaultPosition;
    }

    private CanvasGroup GetPanel(TabType tab)
    {
        switch(tab)
        {
            case TabType.Menu:
                return menuPanel;

            case TabType.Control:
                return controlPanel;

            case TabType.Sound:
                return soundPanel;

            default:
                return null;
        }
    }

    private Vector2 GetPanelDefaultPosition(
        TabType tab)
    {
        switch(tab)
        {
            case TabType.Menu:
                return menuPanelDefaultPosition;

            case TabType.Control:
                return controlPanelDefaultPosition;

            case TabType.Sound:
                return soundPanelDefaultPosition;

            default:
                return Vector2.zero;
        }
    }

    private int GetTabOrder(TabType tab)
    {
        switch(tab)
        {
            case TabType.Menu:
                return 0;

            case TabType.Control:
                return 1;

            case TabType.Sound:
                return 2;

            default:
                return 0;
        }
    }

    private void UpdateTabVisuals(bool immediate)
    {
        UpdateSingleTabVisual(
            menuTabButton,
            currentTab == TabType.Menu,
            immediate
        );

        UpdateSingleTabVisual(
            controlTabButton,
            currentTab == TabType.Control,
            immediate
        );

        UpdateSingleTabVisual(
            soundTabButton,
            currentTab == TabType.Sound,
            immediate
        );
    }

    private void UpdateSingleTabVisual(
        Button button,
        bool isActive,
        bool immediate)
    {
        if(button == null)
            return;

        button.interactable = !isActive;

        RectTransform rect =
            button.GetComponent<RectTransform>();

        if(rect == null)
            return;

        rect.DOKill();

        Vector3 targetScale =
            Vector3.one *
            (isActive
                ? activeTabScale
                : inactiveTabScale);

        if(immediate)
        {
            rect.localScale = targetScale;
            return;
        }

        rect.DOScale(
                targetScale,
                0.15f
            )
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void SetAllButtonsInteractable(
        bool interactable)
    {
        if(menuTabButton != null)
        {
            menuTabButton.interactable =
                interactable &&
                currentTab != TabType.Menu;
        }

        if(controlTabButton != null)
        {
            controlTabButton.interactable =
                interactable &&
                currentTab != TabType.Control;
        }

        if(soundTabButton != null)
        {
            soundTabButton.interactable =
                interactable &&
                currentTab != TabType.Sound;
        }

        if(restartDayButton != null)
        {
            restartDayButton.interactable =
                interactable;
        }

        if(returnTitleButton != null)
        {
            returnTitleButton.interactable =
                interactable;
        }

        if(continueButton != null)
        {
            continueButton.interactable =
                interactable;
        }
    }

    private void KillTweens()
    {
        dimmedBackground?.DOKill();
        windowRoot?.DOKill();

        menuPanel?.DOKill();
        controlPanel?.DOKill();
        soundPanel?.DOKill();

        menuTabButton?
            .GetComponent<RectTransform>()?
            .DOKill();

        controlTabButton?
            .GetComponent<RectTransform>()?
            .DOKill();

        soundTabButton?
            .GetComponent<RectTransform>()?
            .DOKill();
    }
}