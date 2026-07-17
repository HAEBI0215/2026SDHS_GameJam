using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용")]
    [SerializeField]
    private KeyCode interactKey = KeyCode.E;

    [SerializeField]
    private float interactRange = 1.5f;

    [Tooltip("비워두면 플레이어 위치를 기준으로 검사합니다.")]
    [SerializeField]
    private Transform interactionCenter;

    [Tooltip("상호작용 가능한 오브젝트 레이어")]
    [SerializeField]
    private LayerMask interactableLayer = ~0;

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();

        if(inventory == null)
        {
            Debug.LogError(
                $"{name}: PlayerInventory가 없습니다."
            );
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void Interact()
    {
        if(inventory == null)
            return;

        Vector2 center =
            interactionCenter != null
                ? interactionCenter.position
                : transform.position;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                center,
                interactRange,
                interactableLayer
            );

        IInteractable bestInteractable =
            FindBestInteractable(
                hits,
                center
            );

        if(bestInteractable != null)
        {
            bestInteractable.Interact(
                inventory
            );

            return;
        }

        // 주변에 상호작용 대상이 없을 때만
        // 들고 있는 아이템 내려놓기
        if(inventory.HasItem())
        {
            inventory.Drop();
        }
    }

    private IInteractable FindBestInteractable(
        Collider2D[] hits,
        Vector2 center)
    {
        IInteractable bestInteractable =
            null;

        float bestScore =
            float.MaxValue;

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
                continue;

            // 콜라이더가 자식에 붙어 있어도
            // 부모의 상호작용 스크립트를 찾음
            IInteractable interactable =
                hit.GetComponent<IInteractable>();

            if(interactable == null)
            {
                interactable =
                    hit.GetComponentInParent<
                        IInteractable
                    >();
            }

            if(interactable == null)
                continue;

            MonoBehaviour targetBehaviour =
                interactable as MonoBehaviour;

            if(targetBehaviour == null)
                continue;

            // 자기 자신 제외
            if(targetBehaviour.gameObject ==
               gameObject)
            {
                continue;
            }

            int priority =
                GetInteractionPriority(
                    interactable
                );

            Vector2 closestPoint =
                hit.ClosestPoint(center);

            float distance =
                Vector2.Distance(
                    center,
                    closestPoint
                );

            // 우선순위 차이가 거리보다 훨씬 크게 반영됨
            float score =
                priority * 100f +
                distance;

            if(score >= bestScore)
                continue;

            bestScore = score;

            bestInteractable =
                interactable;
        }

        return bestInteractable;
    }

    private int GetInteractionPriority(
    IInteractable interactable)
{
    // 빈손이면 완성 음식과 실패 음식을 가장 먼저 줍기
    if(interactable is FoodItem)
    {
        return inventory.HasItem()
            ? 50
            : 0;
    }

    // 바닥의 일반 아이템
    if(interactable is ItemBase)
    {
        return inventory.HasItem()
            ? 40
            : 1;
    }

    // 조리대, 서빙대, 쓰레기통 등
    return 10;
}

    private void OnDrawGizmosSelected()
    {
        Vector3 center =
            interactionCenter != null
                ? interactionCenter.position
                : transform.position;

        Gizmos.DrawWireSphere(
            center,
            interactRange
        );
    }
}