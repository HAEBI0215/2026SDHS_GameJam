using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource audioSource;

    // BGM
    [Header("BGM")]
    public AudioClip bgm;

    // Cooking
    [Header("Cooking Sounds")]
    public AudioClip cookingSound;
    public AudioClip slicingSound;
    public AudioClip makeFoodSound;
    public AudioClip failSound;

    // Food Interaction
    [Header("Food Interaction Sounds")]
    public AudioClip grabSound;
    public AudioClip setPlateSound;
    public AudioClip wasteFoodSound;

    // UI / System
    [Header("UI & System Sounds")]
    public AudioClip receiptSound;
    public AudioClip coinSound;

    // Result
    [Header("Result Sounds")]
    public AudioClip finishSound;

    // Environment
    [Header("Environment Sounds")]
    public AudioClip openDoorSound;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void BGMVolume(float value)
    {
        audioSource.volume = value;
    }

    public void PlaySFX(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void SFXVolume(float value)
    {
        audioSource.volume = value;
    }
}