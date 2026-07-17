using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OrderReceiptUIManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private RectTransform container;

    [SerializeField]
    private ReceiptOrderUI receiptPrefab;

    [Header("기본 영수증 배치")]
    [Tooltip("서로 떨어져 보이는 기본 영수증 개수")]
    [SerializeField]
    private int visibleCount = 3;

    [Tooltip("첫 번째 영수증이 도착할 위치. 가장 왼쪽 슬롯")]
    [SerializeField]
    private Vector2 firstVisiblePosition =
        new Vector2(-420f, 0f);

    [Tooltip("1~3번째 영수증 사이의 가로 간격")]
    [SerializeField]
    private float visibleSpacingX = 210f;

    [Header("초과 주문 배치")]
    [Tooltip("초과 영수증과 컨테이너 오른쪽 가장자리 사이 간격")]
    [SerializeField]
    private float overflowRightPadding = 10f;

    [Tooltip("초과 영수증이 서로 겹치는 간격")]
    [SerializeField]
    private Vector2 overflowStackOffset =
        new Vector2(-14f, -3f);

    [Header("일반 재배치 애니메이션")]
    [SerializeField]
    private float layoutMoveDuration = 0.25f;

    [SerializeField]
    private Ease layoutMoveEase = Ease.OutCubic;

    [Header("영수증 입장 애니메이션")]
    [Tooltip("오른쪽 화면 밖으로 숨길 추가 거리")]
    [SerializeField]
    private float enterOutsidePadding = 40f;

    [Tooltip("철봉을 타고 이동할 때 목표보다 위에 있는 높이")]
    [SerializeField]
    private float enterLiftY = 16f;

    [Tooltip("영수증이 옆으로 이동하는 속도")]
    [SerializeField]
    private float enterMoveSpeed = 1200f;

    [SerializeField]
    private float minimumEnterDuration = 0.18f;

    [SerializeField]
    private float maximumEnterDuration = 0.65f;

    [Tooltip("철봉에 아래로 걸리는 시간")]
    [SerializeField]
    private float hangDuration = 0.14f;

    [Tooltip("철봉에 걸린 뒤 흔들리는 각도")]
    [SerializeField]
    private float hangSwingAngle = 3f;

    [Header("영수증 뜯기 애니메이션")]
    [Tooltip("처음 뜯을 때 당기는 방향")]
    [SerializeField]
    private Vector2 tearPullOffset =
        new Vector2(30f, -45f);

    [Tooltip("뜯긴 후 화면 밖으로 떨어지는 방향")]
    [SerializeField]
    private Vector2 tearFlyOffset =
        new Vector2(-260f, -1000f);

    [SerializeField]
    private float tearPullDuration = 0.09f;

    [SerializeField]
    private float tearFlyDuration = 0.55f;

    [SerializeField]
    private float tearRotation = -25f;

    private readonly List<ReceiptOrderUI> receipts =
        new List<ReceiptOrderUI>();

    private bool isSubscribed;

    private void Start()
    {
        ResolveOrderManager();
        SubscribeEvents();
        CreateExistingOrderReceipts();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        KillAllTweens();
    }

    private void ResolveOrderManager()
    {
        if(orderManager != null)
            return;

        if(GameManager.Instance == null)
        {
            Debug.LogError(
                "GameManager.Instance를 찾을 수 없습니다."
            );

            return;
        }

        orderManager =
            GameManager.Instance.Order;
    }

    private void SubscribeEvents()
    {
        if(orderManager == null)
        {
            Debug.LogError(
                "OrderReceiptUIManager에 OrderManager가 연결되지 않았습니다."
            );

            return;
        }

        if(isSubscribed)
            return;

        orderManager.OnOrderAdded +=
            HandleOrderAdded;

        orderManager.OnOrderCompleted +=
            HandleOrderRemoved;

        orderManager.OnOrderExpired +=
            HandleOrderRemoved;

        orderManager.OnOrdersCleared +=
            ClearAllReceipts;

        isSubscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if(orderManager == null ||
           !isSubscribed)
        {
            return;
        }

        orderManager.OnOrderAdded -=
            HandleOrderAdded;

        orderManager.OnOrderCompleted -=
            HandleOrderRemoved;

        orderManager.OnOrderExpired -=
            HandleOrderRemoved;

        orderManager.OnOrdersCleared -=
            ClearAllReceipts;

        isSubscribed = false;
    }

    private void CreateExistingOrderReceipts()
    {
        if(orderManager == null)
            return;

        IReadOnlyList<ActiveOrder> activeOrders =
            orderManager.ActiveOrders;

        for(int i = 0; i < activeOrders.Count; i++)
        {
            ActiveOrder order =
                activeOrders[i];

            if(order == null)
                continue;

            if(FindReceipt(order) != null)
                continue;

            CreateReceipt(
                order,
                false
            );
        }

        RebuildLayout(null);
    }

    private void HandleOrderAdded(
        ActiveOrder order)
    {
        CreateReceipt(
            order,
            true
        );
    }

    private void CreateReceipt(
        ActiveOrder order,
        bool playEnterAnimation)
    {
        if(order == null)
            return;

        if(receiptPrefab == null ||
           container == null)
        {
            Debug.LogError(
                "Receipt Prefab 또는 Container가 연결되지 않았습니다."
            );

            return;
        }

        ReceiptOrderUI receipt =
            Instantiate(
                receiptPrefab,
                container
            );

        receipt.Setup(order);

        RectTransform rect =
            receipt.GetComponent<RectTransform>();

        if(rect == null)
        {
            Debug.LogError(
                "Receipt 프리팹에 RectTransform이 없습니다."
            );

            Destroy(receipt.gameObject);
            return;
        }

        // 영수증 위쪽 중앙이 철봉에 걸리도록 설정
        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 1f);

        rect.localScale =
            Vector3.one;

        rect.localRotation =
            Quaternion.identity;

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            rect
        );

        receipts.Add(receipt);

        int newIndex =
            receipts.Count - 1;

        Vector2 targetPosition =
            GetTargetPosition(
                newIndex,
                rect
            );

        // 기존 영수증 위치 정리
        RebuildLayout(receipt);

        UpdateAllReceiptLayers();

        if(playEnterAnimation)
        {
            PlayEnterAnimation(
                receipt,
                targetPosition
            );
        }
        else
        {
            rect.anchoredPosition =
                targetPosition;
        }
    }

    private void PlayEnterAnimation(
        ReceiptOrderUI receipt,
        Vector2 targetPosition)
    {
        if(receipt == null)
            return;

        RectTransform rect =
            receipt.GetComponent<RectTransform>();

        if(rect == null)
            return;

        rect.DOKill();

        Vector2 railTargetPosition =
            targetPosition +
            new Vector2(
                0f,
                enterLiftY
            );

        Vector2 startPosition =
            GetRightOutsidePosition(
                rect,
                railTargetPosition.y
            );

        rect.anchoredPosition =
            startPosition;

        rect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                -hangSwingAngle
            );

        float horizontalDistance =
            Mathf.Abs(
                startPosition.x -
                railTargetPosition.x
            );

        float slideDuration =
            horizontalDistance /
            Mathf.Max(1f, enterMoveSpeed);

        slideDuration =
            Mathf.Clamp(
                slideDuration,
                minimumEnterDuration,
                maximumEnterDuration
            );

        Sequence sequence =
            DOTween.Sequence();

        // 오른쪽 화면 밖에서 왼쪽 목표 위치까지 이동
        sequence.Append(
            rect.DOAnchorPosX(
                    railTargetPosition.x,
                    slideDuration
                )
                .SetEase(Ease.OutCubic)
        );

        sequence.Join(
            rect.DOAnchorPosY(
                    railTargetPosition.y,
                    slideDuration
                )
                .SetEase(Ease.OutSine)
        );

        sequence.Join(
            rect.DORotate(
                    Vector3.zero,
                    slideDuration
                )
                .SetEase(Ease.OutSine)
        );

        // 철봉 위치에 도착한 뒤 아래로 툭 걸림
        sequence.Append(
            rect.DOAnchorPosY(
                    targetPosition.y,
                    hangDuration
                )
                .SetEase(Ease.OutBack)
        );

        sequence.Join(
            rect.DORotate(
                    new Vector3(
                        0f,
                        0f,
                        hangSwingAngle
                    ),
                    hangDuration
                )
                .SetEase(Ease.OutSine)
        );

        // 걸린 뒤 흔들림 정리
        sequence.Append(
            rect.DORotate(
                    new Vector3(
                        0f,
                        0f,
                        -hangSwingAngle * 0.45f
                    ),
                    0.08f
                )
                .SetEase(Ease.InOutSine)
        );

        sequence.Append(
            rect.DORotate(
                    Vector3.zero,
                    0.1f
                )
                .SetEase(Ease.OutSine)
        );
    }

    private Vector2 GetRightOutsidePosition(
        RectTransform receiptRect,
        float targetY)
    {
        float receiptWidth =
            receiptRect.rect.width;

        /*
         * 영수증의 왼쪽 끝이 컨테이너 오른쪽 끝보다
         * 바깥에 있도록 배치한다.
         */
        float startX =
            container.rect.xMax +
            enterOutsidePadding +
            receiptWidth *
            receiptRect.pivot.x;

        return new Vector2(
            startX,
            targetY
        );
    }

    private Vector2 GetTargetPosition(
        int index,
        RectTransform receiptRect)
    {
        // 1~3번째는 왼쪽부터 오른쪽까지 떨어뜨려 배치
        if(index < visibleCount)
        {
            return firstVisiblePosition +
                   new Vector2(
                       visibleSpacingX * index,
                       0f
                   );
        }

        /*
         * 4번째부터는 컨테이너 오른쪽 가장자리에
         * 영수증 오른쪽 끝이 맞도록 배치한다.
         */
        float receiptWidth =
            receiptRect.rect.width;

        float rightAlignedX =
            container.rect.xMax -
            overflowRightPadding -
            receiptWidth *
            (1f - receiptRect.pivot.x);

        int overflowIndex =
            index - visibleCount;

        return new Vector2(
                   rightAlignedX,
                   firstVisiblePosition.y
               ) +
               overflowStackOffset *
               overflowIndex;
    }

    private void HandleOrderRemoved(
        ActiveOrder order)
    {
        ReceiptOrderUI targetReceipt =
            FindReceipt(order);

        if(targetReceipt == null)
            return;

        receipts.Remove(
            targetReceipt
        );

        // 빠진 자리로 뒤 주문들을 이동
        RebuildLayout(null);
        UpdateAllReceiptLayers();

        // 완료·실패 영수증은 뜯어서 제거
        PlayTearAnimation(
            targetReceipt
        );
    }

    private ReceiptOrderUI FindReceipt(
        ActiveOrder order)
    {
        for(int i = 0; i < receipts.Count; i++)
        {
            ReceiptOrderUI receipt =
                receipts[i];

            if(receipt == null)
                continue;

            if(receipt.TargetOrder == order)
                return receipt;
        }

        return null;
    }

    private void PlayTearAnimation(
        ReceiptOrderUI receipt)
    {
        if(receipt == null)
            return;

        RectTransform rect =
            receipt.GetComponent<RectTransform>();

        if(rect == null)
        {
            Destroy(receipt.gameObject);
            return;
        }

        rect.DOKill();

        // 뜯기는 영수증을 다른 영수증 앞으로 표시
        rect.SetAsLastSibling();

        Vector2 startPosition =
            rect.anchoredPosition;

        Vector2 pullPosition =
            startPosition +
            tearPullOffset;

        Vector2 endPosition =
            pullPosition +
            tearFlyOffset;

        Sequence sequence =
            DOTween.Sequence();

        // 철봉에서 순간적으로 뜯김
        sequence.Append(
            rect.DOAnchorPos(
                    pullPosition,
                    tearPullDuration
                )
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            rect.DORotate(
                    new Vector3(
                        0f,
                        0f,
                        tearRotation * 0.25f
                    ),
                    tearPullDuration
                )
                .SetEase(Ease.InQuad)
        );

        // 대각선 아래로 떨어짐
        sequence.Append(
            rect.DOAnchorPos(
                    endPosition,
                    tearFlyDuration
                )
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            rect.DORotate(
                    new Vector3(
                        0f,
                        0f,
                        tearRotation
                    ),
                    tearFlyDuration
                )
                .SetEase(Ease.InQuad)
        );

        sequence.OnComplete(() =>
        {
            if(receipt != null)
            {
                Destroy(
                    receipt.gameObject
                );
            }
        });
    }

    private void RebuildLayout(
        ReceiptOrderUI ignoredReceipt)
    {
        for(int i = 0; i < receipts.Count; i++)
        {
            ReceiptOrderUI receipt =
                receipts[i];

            if(receipt == null ||
               receipt == ignoredReceipt)
            {
                continue;
            }

            RectTransform rect =
                receipt.GetComponent<RectTransform>();

            if(rect == null)
                continue;

            Vector2 targetPosition =
                GetTargetPosition(
                    i,
                    rect
                );

            rect.DOKill();

            rect.DOAnchorPos(
                    targetPosition,
                    layoutMoveDuration
                )
                .SetEase(layoutMoveEase);

            rect.DORotate(
                    Vector3.zero,
                    layoutMoveDuration
                )
                .SetEase(layoutMoveEase);
        }
    }

    private void UpdateAllReceiptLayers()
    {
        for(int i = 0; i < receipts.Count; i++)
        {
            ReceiptOrderUI receipt =
                receipts[i];

            if(receipt == null)
                continue;

            RectTransform rect =
                receipt.GetComponent<RectTransform>();

            if(rect == null)
                continue;

            /*
             * 나중에 생성된 영수증이 위에 렌더링돼서
             * 초과 주문이 겹쳐진 것처럼 보인다.
             */
            rect.SetSiblingIndex(
                Mathf.Clamp(
                    i,
                    0,
                    container.childCount - 1
                )
            );
        }
    }

    private void ClearAllReceipts()
    {
        for(int i = 0; i < receipts.Count; i++)
        {
            ReceiptOrderUI receipt =
                receipts[i];

            if(receipt == null)
                continue;

            RectTransform rect =
                receipt.GetComponent<RectTransform>();

            if(rect != null)
            {
                rect.DOKill();
            }

            Destroy(
                receipt.gameObject
            );
        }

        receipts.Clear();
    }

    private void KillAllTweens()
    {
        for(int i = 0; i < receipts.Count; i++)
        {
            ReceiptOrderUI receipt =
                receipts[i];

            if(receipt == null)
                continue;

            RectTransform rect =
                receipt.GetComponent<RectTransform>();

            if(rect != null)
            {
                rect.DOKill();
            }
        }
    }
}