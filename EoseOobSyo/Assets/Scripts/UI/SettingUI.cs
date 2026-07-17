using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingUI : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    public TMP_InputField bgmInput;
    public TMP_InputField sfxInput;

    void Start()
    {
        bgmSlider.value = 1;
        sfxSlider.value = 1;

        bgmInput.text = "100";
        sfxInput.text = "100";

        bgmSlider.onValueChanged.AddListener(SetBGM);
        sfxSlider.onValueChanged.AddListener(SetSFX);

        bgmInput.onEndEdit.AddListener(SetBGMInput);
        sfxInput.onEndEdit.AddListener(SetSFXInput);
    }

    void SetBGM(float value)
    {
        SoundManager.Instance.BGMVolume(value);

        bgmInput.text = Mathf.RoundToInt(value * 100).ToString();
    }


    void SetSFX(float value)
    {
        SoundManager.Instance.SFXVolume(value);

        sfxInput.text = Mathf.RoundToInt(value * 100).ToString();
    }


    // InputField 입력
    void SetBGMInput(string text)
    {
        float value = Mathf.Clamp(float.Parse(text) / 100f, 0, 1);

        bgmSlider.value = value;

        SoundManager.Instance.BGMVolume(value);
    }


    void SetSFXInput(string text)
    {
        float value = Mathf.Clamp(float.Parse(text) / 100f, 0, 1);

        sfxSlider.value = value;

        SoundManager.Instance.SFXVolume(value);
    }
}
