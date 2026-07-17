using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonTween :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField]
    private Button targetButton;

    [Header("크기")]
    [SerializeField]
    private float hoverScale = 1.07f;

    [SerializeField]
    private float pressedScale = 0.94f;

    [Header("시간")]
    [SerializeField]
    private float animationDuration = 0.14f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform =
            transform as RectTransform;

        if(targetButton == null)
        {
            targetButton =
                GetComponent<Button>();
        }
    }

    private void OnDisable()
    {
        if(rectTransform == null)
            return;

        rectTransform.DOKill();

        rectTransform.localScale =
            Vector3.one;
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if(!CanAnimate())
            return;

        AnimateScale(
            hoverScale,
            Ease.OutBack
        );
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        AnimateScale(
            1f,
            Ease.OutQuad
        );
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        if(!CanAnimate())
            return;

        AnimateScale(
            pressedScale,
            Ease.OutQuad
        );
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        if(!CanAnimate())
        {
            AnimateScale(
                1f,
                Ease.OutQuad
            );

            return;
        }

        AnimateScale(
            hoverScale,
            Ease.OutBack
        );
    }

    private bool CanAnimate()
    {
        return targetButton == null ||
               targetButton.interactable;
    }

    private void AnimateScale(
        float targetScale,
        Ease ease)
    {
        if(rectTransform == null)
            return;

        rectTransform.DOKill();

        rectTransform
            .DOScale(
                targetScale,
                animationDuration
            )
            .SetEase(ease)
            .SetUpdate(true);
    }
}