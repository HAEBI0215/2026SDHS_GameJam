using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUI : MonoBehaviour
{
    [Header("타이틀 UI")]
    [SerializeField]
    private CanvasGroup titleCanvasGroup;

    [SerializeField]
    private RectTransform logo;

    [SerializeField]
    private RectTransform buttonGroup;

    [SerializeField]
    private RectTransform[] menuButtons;

    [Header("버튼")]
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button[] allButtons;

    [Header("씬")]
    [SerializeField]
    private string inGameSceneName =
        "InGame";

    [Header("연출 시간")]
    [SerializeField]
    private float logoDuration = 0.55f;

    [SerializeField]
    private float buttonDuration = 0.28f;

    [SerializeField]
    private float buttonDelay = 0.08f;

    private Sequence titleSequence;

    private Vector2 buttonGroupOriginalPosition;

    private bool isStartingGame;

    private void Awake()
    {
        Time.timeScale = 1f;

        if(buttonGroup != null)
        {
            buttonGroupOriginalPosition =
                buttonGroup
                    .anchoredPosition;
        }

        if(startButton != null)
        {
            startButton.onClick
                .AddListener(
                    StartGame
                );
        }
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    private void OnDestroy()
    {
        titleSequence?.Kill();

        if(startButton != null)
        {
            startButton.onClick
                .RemoveListener(
                    StartGame
                );
        }
    }

    private void PlayIntroAnimation()
    {
        titleSequence?.Kill();

        if(titleCanvasGroup == null)
            return;

        titleCanvasGroup.alpha = 0f;
        titleCanvasGroup.interactable =
            false;

        titleCanvasGroup.blocksRaycasts =
            false;

        if(logo != null)
        {
            logo.localScale =
                Vector3.one * 0.55f;

            logo.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -8f
                );
        }

        if(buttonGroup != null)
        {
            buttonGroup.anchoredPosition =
                buttonGroupOriginalPosition +
                Vector2.down * 40f;
        }

        if(menuButtons != null)
        {
            foreach(
                RectTransform menuButton
                in menuButtons)
            {
                if(menuButton == null)
                    continue;

                menuButton.localScale =
                    Vector3.one * 0.75f;
            }
        }

        titleSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        titleSequence.Append(
            titleCanvasGroup
                .DOFade(
                    1f,
                    0.32f
                )
        );

        if(logo != null)
        {
            titleSequence.Join(
                logo
                    .DOScale(
                        1f,
                        logoDuration
                    )
                    .SetEase(
                        Ease.OutBack
                    )
            );

            titleSequence.Join(
                logo
                    .DORotate(
                        Vector3.zero,
                        logoDuration
                    )
                    .SetEase(
                        Ease.OutCubic
                    )
            );
        }

        if(buttonGroup != null)
        {
            titleSequence.Insert(
                0.25f,
                buttonGroup
                    .DOAnchorPos(
                        buttonGroupOriginalPosition,
                        0.35f
                    )
                    .SetEase(
                        Ease.OutCubic
                    )
            );
        }

        if(menuButtons != null)
        {
            for(int i = 0;
                i < menuButtons.Length;
                i++)
            {
                RectTransform menuButton =
                    menuButtons[i];

                if(menuButton == null)
                    continue;

                float insertTime =
                    0.3f +
                    i * buttonDelay;

                titleSequence.Insert(
                    insertTime,
                    menuButton
                        .DOScale(
                            1f,
                            buttonDuration
                        )
                        .SetEase(
                            Ease.OutBack
                        )
                );
            }
        }

        titleSequence.OnComplete(
            () =>
            {
                titleCanvasGroup
                    .interactable = true;

                titleCanvasGroup
                    .blocksRaycasts = true;
            }
        );
    }

    private void StartGame()
    {
        if(isStartingGame)
            return;

        SceneTransitionManager
            transitionManager =
                SceneTransitionManager
                    .Instance;

        if(transitionManager == null)
        {
            Debug.LogError(
                "SceneTransitionManager가 없습니다."
            );

            return;
        }

        isStartingGame = true;

        SetButtonsInteractable(false);

        titleCanvasGroup.interactable =
            false;

        titleCanvasGroup.blocksRaycasts =
            false;

        titleSequence?.Kill();

        titleSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(startButton != null)
        {
            titleSequence.Append(
                startButton.transform
                    .DOPunchScale(
                        Vector3.one * 0.18f,
                        0.22f,
                        8,
                        0.7f
                    )
            );
        }

        if(logo != null)
        {
            titleSequence.Append(
                logo
                    .DOScale(
                        1.1f,
                        0.16f
                    )
                    .SetEase(
                        Ease.OutQuad
                    )
            );
        }

        if(buttonGroup != null)
        {
            titleSequence.Join(
                buttonGroup
                    .DOAnchorPosY(
                        buttonGroupOriginalPosition.y -
                        35f,
                        0.22f
                    )
                    .SetEase(
                        Ease.InCubic
                    )
            );
        }

        titleSequence.Join(
            titleCanvasGroup
                .DOFade(
                    0f,
                    0.22f
                )
        );

        titleSequence.OnComplete(
            () =>
            {
                transitionManager.LoadScene(
                    inGameSceneName
                );
            }
        );
    }

    private void SetButtonsInteractable(
        bool interactable)
    {
        if(allButtons == null)
            return;

        foreach(Button button in allButtons)
        {
            if(button == null)
                continue;

            button.interactable =
                interactable;
        }
    }
}