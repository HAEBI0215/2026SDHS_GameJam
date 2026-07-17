using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingTimerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject timerPanel;

    [SerializeField]
    private Slider timerSlider;

    [SerializeField]
    private TMP_Text timerText;

    private void Awake()
    {
        if(timerSlider != null)
        {
            timerSlider.interactable = false;
        }

        Hide();
    }

    public void Show(float maxTime)
    {
        if(timerPanel != null)
        {
            timerPanel.SetActive(true);
        }

        if(timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = maxTime;
            timerSlider.value = maxTime;
        }

        if(timerText != null)
        {
            timerText.text = Mathf.CeilToInt(maxTime).ToString();
        }
    }

    public void UpdateTimer(float currentTime, float maxTime)
    {
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        if(timerSlider != null)
        {
            timerSlider.minValue = 0f;
            timerSlider.maxValue = maxTime;
            timerSlider.value = currentTime;
        }

        if(timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    public void Hide()
    {
        if(timerPanel != null)
        {
            timerPanel.SetActive(false);
        }
    }
}