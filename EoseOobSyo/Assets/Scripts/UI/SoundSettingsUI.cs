using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField]
    private Slider masterSlider;

    [SerializeField]
    private Slider vfxSlider;

    [SerializeField]
    private Slider bgmSlider;

    private SoundManager sound;

    private void OnEnable()
    {
        sound = SoundManager.Instance;

        if(sound == null)
        {
            Debug.LogError(
                "SoundManager가 없습니다."
            );

            return;
        }

        SetupSlider(
            masterSlider,
            sound.MasterVolume
        );

        SetupSlider(
            vfxSlider,
            sound.VFXVolume
        );

        SetupSlider(
            bgmSlider,
            sound.BGMVolume
        );

        if(masterSlider != null)
        {
            masterSlider.onValueChanged
                .AddListener(
                    sound.SetMasterVolume
                );
        }

        if(vfxSlider != null)
        {
            vfxSlider.onValueChanged
                .AddListener(
                    sound.SetVFXVolume
                );
        }

        if(bgmSlider != null)
        {
            bgmSlider.onValueChanged
                .AddListener(
                    sound.SetBGMVolume
                );
        }
    }

    private void OnDisable()
    {
        if(sound == null)
            return;

        if(masterSlider != null)
        {
            masterSlider.onValueChanged
                .RemoveListener(
                    sound.SetMasterVolume
                );
        }

        if(vfxSlider != null)
        {
            vfxSlider.onValueChanged
                .RemoveListener(
                    sound.SetVFXVolume
                );
        }

        if(bgmSlider != null)
        {
            bgmSlider.onValueChanged
                .RemoveListener(
                    sound.SetBGMVolume
                );
        }

        PlayerPrefs.Save();
    }

    private void SetupSlider(
        Slider slider,
        float value)
    {
        if(slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.SetValueWithoutNotify(
            value
        );
    }
}