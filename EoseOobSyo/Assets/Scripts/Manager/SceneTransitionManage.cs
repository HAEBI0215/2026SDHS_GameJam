using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance
    {
        get;
        private set;
    }

    [Header("전환 UI")]
    [SerializeField]
    private Canvas transitionCanvas;

    [SerializeField]
    private CanvasGroup transitionCanvasGroup;

    [SerializeField]
    private RectTransform leftCurtain;

    [SerializeField]
    private RectTransform rightCurtain;

    [Header("중앙 연출")]
    [SerializeField]
    private RectTransform centerGroup;

    [SerializeField]
    private RectTransform foodIcon;

    [SerializeField]
    private TMP_Text loadingText;

    [Header("문구")]
    [SerializeField]
    private string loadingMessage =
        "영업 준비 중...";

    [SerializeField]
    private string completeMessage =
        "영업 시작!";

    [Header("시간")]
    [SerializeField]
    private float curtainCloseDuration =
        0.55f;

    [SerializeField]
    private float curtainOpenDuration =
        0.55f;

    [SerializeField]
    private float minimumLoadingDuration =
        0.35f;

    [SerializeField]
    private float completeMessageDuration =
        0.3f;

    [Header("커튼 보정")]
    [SerializeField]
    private float curtainOpenPadding =
        20f;

    [Header("아이콘 회전")]
    [SerializeField]
    private float iconRotationDuration =
        0.7f;

    private float leftClosedPosition;
    private float rightClosedPosition;

    private float leftOpenPosition;
    private float rightOpenPosition;

    private bool isTransitioning;

    private Sequence transitionSequence;
    private Tween iconRotationTween;

    public bool IsTransitioning =>
        isTransitioning;

    private void Awake()
    {
        if(Instance != null &&
           Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if(transitionCanvas != null)
        {
            transitionCanvas.overrideSorting =
                true;

            transitionCanvas.sortingOrder =
                999;
        }

        Canvas.ForceUpdateCanvases();

        CacheClosedCurtainPositions();
        CalculateOpenCurtainPositions();

        HideImmediate();
    }

    private void OnDestroy()
    {
        transitionSequence?.Kill();
        iconRotationTween?.Kill();

        if(Instance == this)
        {
            Instance = null;
        }
    }

    public void LoadScene(string sceneName)
    {
        if(isTransitioning)
            return;

        if(string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "이동할 씬 이름이 없습니다."
            );

            return;
        }

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }

    private IEnumerator LoadSceneRoutine(
        string sceneName)
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        PrepareTransition();

        // 1. 커튼부터 완전히 닫기
        transitionSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        transitionSequence.Append(
            leftCurtain
                .DOAnchorPosX(
                    leftClosedPosition,
                    curtainCloseDuration
                )
                .SetEase(Ease.InOutCubic)
        );

        transitionSequence.Join(
            rightCurtain
                .DOAnchorPosX(
                    rightClosedPosition,
                    curtainCloseDuration
                )
                .SetEase(Ease.InOutCubic)
        );

        yield return transitionSequence
            .WaitForCompletion();

        // 닫힘 위치를 확실하게 고정
        SetCurtainsClosed();

        // 2. 완전히 닫힌 후 중앙 UI 등장
        transitionSequence?.Kill();

        transitionSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(centerGroup != null)
        {
            transitionSequence.Append(
                centerGroup
                    .DOScale(
                        1f,
                        0.25f
                    )
                    .SetEase(Ease.OutBack)
            );
        }

        if(loadingText != null)
        {
            transitionSequence.Join(
                loadingText
                    .DOFade(
                        1f,
                        0.18f
                    )
            );
        }

        yield return transitionSequence
            .WaitForCompletion();

        // 얼굴 이미지만 계속 회전
        StartIconRotation();

        // 3. 커튼이 닫힌 상태에서 씬 로드
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single
            );

        if(loadOperation == null)
        {
            Debug.LogError(
                $"{sceneName} 씬을 불러오지 못했습니다."
            );

            StopIconRotation();
            HideImmediate();

            isTransitioning = false;

            yield break;
        }

        loadOperation.allowSceneActivation =
            false;

        float loadingTimer = 0f;

        while(loadOperation.progress < 0.9f ||
              loadingTimer <
              minimumLoadingDuration)
        {
            loadingTimer +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        loadOperation.allowSceneActivation =
            true;

        while(!loadOperation.isDone)
        {
            yield return null;
        }

        // 새 씬이 그려질 때까지 대기
        yield return null;
        yield return null;

        if(loadingText != null)
        {
            loadingText.text =
                completeMessage;
        }

        if(centerGroup != null)
        {
            centerGroup
                .DOPunchScale(
                    Vector3.one * 0.15f,
                    0.3f,
                    7,
                    0.6f
                )
                .SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(
            completeMessageDuration
        );

        StopIconRotation();

        // 4. 중앙 UI 숨기기
        transitionSequence?.Kill();

        transitionSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        if(loadingText != null)
        {
            transitionSequence.Append(
                loadingText
                    .DOFade(
                        0f,
                        0.15f
                    )
            );
        }

        if(centerGroup != null)
        {
            transitionSequence.Join(
                centerGroup
                    .DOScale(
                        0f,
                        0.2f
                    )
                    .SetEase(Ease.InBack)
            );
        }

        yield return transitionSequence
            .WaitForCompletion();

        // 열기 직전에도 닫힘 위치 강제 적용
        SetCurtainsClosed();

        // 5. 커튼 완전히 열기
        transitionSequence?.Kill();

        transitionSequence =
            DOTween.Sequence()
                .SetUpdate(true);

        transitionSequence.Append(
            leftCurtain
                .DOAnchorPosX(
                    leftOpenPosition,
                    curtainOpenDuration
                )
                .SetEase(Ease.InOutCubic)
        );

        transitionSequence.Join(
            rightCurtain
                .DOAnchorPosX(
                    rightOpenPosition,
                    curtainOpenDuration
                )
                .SetEase(Ease.InOutCubic)
        );

        yield return transitionSequence
            .WaitForCompletion();

        HideImmediate();

        isTransitioning = false;
    }

    private void PrepareTransition()
    {
        transitionCanvasGroup.gameObject
            .SetActive(true);

        Canvas.ForceUpdateCanvases();

        CalculateOpenCurtainPositions();

        transitionCanvasGroup.alpha = 1f;
        transitionCanvasGroup.blocksRaycasts =
            true;

        SetCurtainsOpen();

        if(centerGroup != null)
        {
            centerGroup.localScale =
                Vector3.zero;
        }

        if(foodIcon != null)
        {
            foodIcon.localRotation =
                Quaternion.identity;
        }

        if(loadingText != null)
        {
            loadingText.text =
                loadingMessage;

            Color color =
                loadingText.color;

            color.a = 0f;

            loadingText.color =
                color;
        }
    }

    private void CacheClosedCurtainPositions()
    {
        if(leftCurtain != null)
        {
            leftClosedPosition =
                leftCurtain.anchoredPosition.x;
        }

        if(rightCurtain != null)
        {
            rightClosedPosition =
                rightCurtain.anchoredPosition.x;
        }
    }

    private void CalculateOpenCurtainPositions()
    {
        if(leftCurtain != null)
        {
            leftOpenPosition =
                leftClosedPosition -
                leftCurtain.rect.width -
                curtainOpenPadding;
        }

        if(rightCurtain != null)
        {
            rightOpenPosition =
                rightClosedPosition +
                rightCurtain.rect.width +
                curtainOpenPadding;
        }
    }

    private void SetCurtainsClosed()
    {
        if(leftCurtain != null)
        {
            leftCurtain.anchoredPosition =
                new Vector2(
                    leftClosedPosition,
                    leftCurtain
                        .anchoredPosition.y
                );
        }

        if(rightCurtain != null)
        {
            rightCurtain.anchoredPosition =
                new Vector2(
                    rightClosedPosition,
                    rightCurtain
                        .anchoredPosition.y
                );
        }
    }

    private void SetCurtainsOpen()
    {
        if(leftCurtain != null)
        {
            leftCurtain.anchoredPosition =
                new Vector2(
                    leftOpenPosition,
                    leftCurtain
                        .anchoredPosition.y
                );
        }

        if(rightCurtain != null)
        {
            rightCurtain.anchoredPosition =
                new Vector2(
                    rightOpenPosition,
                    rightCurtain
                        .anchoredPosition.y
                );
        }
    }

    private void StartIconRotation()
    {
        if(foodIcon == null)
            return;

        iconRotationTween?.Kill();

        foodIcon.localRotation =
            Quaternion.identity;

        iconRotationTween =
            foodIcon
                .DORotate(
                    new Vector3(
                        0f,
                        0f,
                        -360f
                    ),
                    iconRotationDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.Linear)
                .SetLoops(
                    -1,
                    LoopType.Restart
                )
                .SetUpdate(true);
    }

    private void StopIconRotation()
    {
        iconRotationTween?.Kill();
        iconRotationTween = null;

        if(foodIcon != null)
        {
            foodIcon.localRotation =
                Quaternion.identity;
        }
    }

    private void HideImmediate()
    {
        transitionSequence?.Kill();
        StopIconRotation();

        if(transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;

            transitionCanvasGroup.blocksRaycasts =
                false;

            transitionCanvasGroup.interactable =
                false;

            transitionCanvasGroup.gameObject
                .SetActive(false);
        }
    }
}