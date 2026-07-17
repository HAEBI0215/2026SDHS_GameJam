using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReceiptOrderUI : MonoBehaviour
{
    [Header("주문 정보")]
    [SerializeField]
    private Image foodIcon;

    [SerializeField]
    private TMP_Text recipeNameText;

    [Header("필요 재료 UI")]
    [SerializeField]
    private RectTransform ingredientContainer;

    [SerializeField]
    private Image ingredientIconPrefab;

    [SerializeField]
    private Vector2 ingredientIconSize =
        new Vector2(34f, 34f);

    [SerializeField]
    private float ingredientSpacing = 1f;

    [Header("남은 시간 UI")]
    [SerializeField]
    private TMP_Text timeText;

    [SerializeField]
    private Slider timeSlider;

    private readonly List<Image> spawnedIngredientIcons =
        new List<Image>();

    private ActiveOrder targetOrder;

    public ActiveOrder TargetOrder => targetOrder;

    private void Awake()
    {
        ConfigureIngredientLayout();
        ConfigureTimeSlider();
    }

    public void Setup(ActiveOrder order)
    {
        targetOrder = order;

        if(targetOrder == null ||
           targetOrder.Recipe == null)
        {
            Debug.LogWarning(
                "영수증에 표시할 주문 정보가 없습니다."
            );

            return;
        }

        RecipeData recipe = targetOrder.Recipe;

        SetRecipeInfo(recipe);
        CreateIngredientIcons(recipe.ingredients);
        Refresh();
    }

    private void Update()
    {
        if(targetOrder == null)
            return;

        Refresh();
    }

    private void ConfigureIngredientLayout()
    {
        if(ingredientContainer == null)
            return;

        // 기존 HorizontalLayoutGroup이 있으면 제거
        HorizontalLayoutGroup horizontalLayout =
            ingredientContainer
                .GetComponent<HorizontalLayoutGroup>();

        if(horizontalLayout != null)
        {
            Destroy(horizontalLayout);
        }

        VerticalLayoutGroup verticalLayout =
            ingredientContainer
                .GetComponent<VerticalLayoutGroup>();

        if(verticalLayout == null)
        {
            verticalLayout =
                ingredientContainer.gameObject
                    .AddComponent<VerticalLayoutGroup>();
        }

        verticalLayout.padding =
            new RectOffset(0, 0, 0, 0);

        verticalLayout.spacing =
            ingredientSpacing;

        verticalLayout.childAlignment =
            TextAnchor.UpperCenter;

        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = true;

        verticalLayout.childForceExpandWidth = false;
        verticalLayout.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter =
            ingredientContainer
                .GetComponent<ContentSizeFitter>();

        if(sizeFitter == null)
        {
            sizeFitter =
                ingredientContainer.gameObject
                    .AddComponent<ContentSizeFitter>();
        }

        sizeFitter.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;

        sizeFitter.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
    }

    private void ConfigureTimeSlider()
    {
        if(timeSlider == null)
            return;

        timeSlider.minValue = 0f;
        timeSlider.maxValue = 1f;
        timeSlider.wholeNumbers = false;
        timeSlider.interactable = false;
    }

    private void SetRecipeInfo(RecipeData recipe)
    {
        if(recipeNameText != null)
        {
            recipeNameText.text =
                recipe.recipeName;
        }

        if(foodIcon == null)
            return;

        if(recipe.resultItem == null ||
           recipe.resultItem.icon == null)
        {
            foodIcon.sprite = null;
            foodIcon.enabled = false;

            Debug.LogWarning(
                $"{recipe.recipeName}의 완성 음식 아이콘이 없습니다."
            );

            return;
        }

        foodIcon.enabled = true;
        foodIcon.sprite =
            recipe.resultItem.icon;

        foodIcon.preserveAspect = true;
        foodIcon.raycastTarget = false;

        Color foodColor =
            foodIcon.color;

        foodColor.a = 1f;
        foodIcon.color = foodColor;
    }

    private void CreateIngredientIcons(
        ItemData[] ingredients)
    {
        ClearIngredientIcons();

        if(ingredientContainer == null)
        {
            Debug.LogWarning(
                "Ingredient Container가 연결되지 않았습니다."
            );

            return;
        }

        if(ingredientIconPrefab == null)
        {
            Debug.LogWarning(
                "Ingredient Icon Prefab이 연결되지 않았습니다."
            );

            return;
        }

        if(ingredients == null ||
           ingredients.Length == 0)
        {
            return;
        }

        foreach(ItemData ingredient in ingredients)
        {
            if(ingredient == null)
                continue;

            if(ingredient.icon == null)
            {
                Debug.LogWarning(
                    $"{ingredient.name}의 아이콘이 없습니다."
                );

                continue;
            }

            Image createdIcon =
                Instantiate(
                    ingredientIconPrefab,
                    ingredientContainer,
                    false
                );

            ConfigureIngredientIcon(
                createdIcon,
                ingredient.icon
            );

            spawnedIngredientIcons.Add(
                createdIcon
            );
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder
            .ForceRebuildLayoutImmediate(
                ingredientContainer
            );
    }

    private void ConfigureIngredientIcon(
        Image icon,
        Sprite sprite)
    {
        icon.gameObject.SetActive(true);

        icon.enabled = true;
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        Color iconColor =
            icon.color;

        iconColor.a = 1f;
        icon.color = iconColor;

        RectTransform iconRect =
            icon.rectTransform;

        iconRect.localScale =
            Vector3.one;

        iconRect.sizeDelta =
            ingredientIconSize;

        LayoutElement layoutElement =
            icon.GetComponent<LayoutElement>();

        if(layoutElement == null)
        {
            layoutElement =
                icon.gameObject
                    .AddComponent<LayoutElement>();
        }

        layoutElement.ignoreLayout = false;

        layoutElement.minWidth =
            ingredientIconSize.x;

        layoutElement.minHeight =
            ingredientIconSize.y;

        layoutElement.preferredWidth =
            ingredientIconSize.x;

        layoutElement.preferredHeight =
            ingredientIconSize.y;

        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;
    }

    private void ClearIngredientIcons()
    {
        foreach(Image icon in spawnedIngredientIcons)
        {
            if(icon == null)
                continue;

            // Destroy되기 전까지 레이아웃에 남지 않도록 즉시 비활성화
            icon.gameObject.SetActive(false);
            Destroy(icon.gameObject);
        }

        spawnedIngredientIcons.Clear();
    }

    private void Refresh()
    {
        if(targetOrder == null)
            return;

        if(timeText != null)
        {
            int remainingSeconds =
                Mathf.CeilToInt(
                    targetOrder.RemainingTime
                );

            timeText.text =
                $"{remainingSeconds}초";
        }

        if(timeSlider != null)
        {
            timeSlider.SetValueWithoutNotify(
                targetOrder.RemainingRatio
            );
        }
    }

    private void OnDestroy()
    {
        ClearIngredientIcons();
    }
}