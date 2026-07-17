using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get;
        private set;
    }

    [Header("Audio Source")]
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource vfxSource;

    [Header("씬 이름")]
    [SerializeField]
    private string titleSceneName = "Title";

    [SerializeField]
    private string inGameSceneName = "InGame";

    [Header("BGM")]
    [SerializeField]
    private AudioClip titleBGM;

    [SerializeField]
    private AudioClip inGameBGM;

    [Header("BGM 전환")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    [Header("SFX")]
    [SerializeField]
    private AudioClip moneySound;

    [SerializeField]
    private int moneySoundRepeatCount = 5;

    [SerializeField]
    private float moneySoundInterval = 0.08f;

    private const string MasterVolumeKey =
        "MasterVolume";

    private const string BGMVolumeKey =
        "BGMVolume";

    private const string VFXVolumeKey =
        "VFXVolume";

    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float vfxVolume = 1f;

    private Coroutine bgmCoroutine;

    public float MasterVolume =>
        masterVolume;

    public float BGMVolume =>
        bgmVolume;

    public float VFXVolume =>
        vfxVolume;

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

        LoadVolumeSettings();
        SetupAudioSources();
        ApplyVolumes();
    }

    private void Start()
    {
        SceneManager.sceneLoaded +=
            HandleSceneLoaded;

        PlaySceneBGM(
            SceneManager.GetActiveScene().name,
            true
        );
    }

    private void OnDestroy()
    {
        if(Instance != this)
            return;

        SceneManager.sceneLoaded -=
            HandleSceneLoaded;

        Instance = null;
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        PlaySceneBGM(scene.name);
    }

    private void PlaySceneBGM(
        string sceneName,
        bool immediate = false)
    {
        AudioClip nextClip = null;

        if(sceneName == titleSceneName)
        {
            nextClip = titleBGM;
        }
        else if(sceneName == inGameSceneName)
        {
            nextClip = inGameBGM;
        }

        if(nextClip == null)
            return;

        PlayBGM(nextClip, immediate);
    }

    public void PlayBGM(
        AudioClip clip,
        bool immediate = false)
    {
        if(clip == null ||
           bgmSource == null)
        {
            return;
        }

        if(bgmSource.clip == clip &&
           bgmSource.isPlaying)
        {
            return;
        }

        if(bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
        }

        if(immediate)
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume =
                GetBGMVolume();
            bgmSource.Play();

            return;
        }

        bgmCoroutine =
            StartCoroutine(
                ChangeBGMRoutine(clip)
            );
    }

    private IEnumerator ChangeBGMRoutine(
        AudioClip nextClip)
    {
        if(bgmSource.isPlaying)
        {
            float startVolume =
                bgmSource.volume;

            float timer = 0f;

            while(timer < fadeDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float ratio =
                    fadeDuration <= 0f
                        ? 1f
                        : timer / fadeDuration;

                bgmSource.volume =
                    Mathf.Lerp(
                        startVolume,
                        0f,
                        ratio
                    );

                yield return null;
            }
        }

        bgmSource.Stop();
        bgmSource.clip = nextClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float fadeTimer = 0f;

        while(fadeTimer < fadeDuration)
        {
            fadeTimer +=
                Time.unscaledDeltaTime;

            float ratio =
                fadeDuration <= 0f
                    ? 1f
                    : fadeTimer / fadeDuration;

            // 페이드 중 슬라이더를 움직여도 반영
            bgmSource.volume =
                Mathf.Lerp(
                    0f,
                    GetBGMVolume(),
                    ratio
                );

            yield return null;
        }

        bgmSource.volume =
            GetBGMVolume();

        bgmCoroutine = null;
    }

    public void StopBGM()
    {
        if(bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }

        if(bgmSource == null)
            return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlayVFX(
        AudioClip clip,
        float volumeScale = 1f)
    {
        if(clip == null ||
           vfxSource == null)
        {
            return;
        }

        vfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeScale)
        );
    }

    public void SetMasterVolume(float value)
    {
        masterVolume =
            Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        ApplyVolumes();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume =
            Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            BGMVolumeKey,
            bgmVolume
        );

        ApplyVolumes();
    }

    public void SetVFXVolume(float value)
    {
        vfxVolume =
            Mathf.Clamp01(value);

        PlayerPrefs.SetFloat(
            VFXVolumeKey,
            vfxVolume
        );

        ApplyVolumes();
    }

    public void ResetVolumes()
    {
        SetMasterVolume(1f);
        SetBGMVolume(1f);
        SetVFXVolume(1f);

        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        if(bgmSource != null)
        {
            bgmSource.volume =
                GetBGMVolume();
        }

        if(vfxSource != null)
        {
            vfxSource.volume =
                GetVFXVolume();
        }
    }

    private float GetBGMVolume()
    {
        return masterVolume *
               bgmVolume;
    }

    private float GetVFXVolume()
    {
        return masterVolume *
               vfxVolume;
    }

    private void LoadVolumeSettings()
    {
        masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        bgmVolume =
            PlayerPrefs.GetFloat(
                BGMVolumeKey,
                1f
            );

        vfxVolume =
            PlayerPrefs.GetFloat(
                VFXVolumeKey,
                1f
            );
    }

    private void SetupAudioSources()
    {
        if(bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }

        if(vfxSource != null)
        {
            vfxSource.playOnAwake = false;
            vfxSource.loop = false;
            vfxSource.spatialBlend = 0f;
        }
    }

    private void OnApplicationPause(
        bool pauseStatus)
    {
        if(pauseStatus)
        {
            PlayerPrefs.Save();
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    public void PlayMoneySound()
    {
        PlayMoneySound(
            moneySoundRepeatCount,
            moneySoundInterval
        );
    }

    public void PlayMoneySound(
        int repeatCount,
        float interval,
        float volumeScale = 1f)
    {
        if(moneySound == null)
        {
            Debug.LogWarning(
                "Money Sound가 연결되지 않았습니다."
            );

            return;
        }

        StartCoroutine(
            PlayMoneySoundRoutine(
                repeatCount,
                interval,
                volumeScale
            )
        );
    }

    private IEnumerator PlayMoneySoundRoutine(
        int repeatCount,
        float interval,
        float volumeScale)
    {
        repeatCount =
            Mathf.Max(1, repeatCount);

        interval =
            Mathf.Max(0f, interval);

        for(int i = 0; i < repeatCount; i++)
        {
            PlayVFX(
                moneySound,
                volumeScale
            );

            if(i < repeatCount - 1)
            {
                yield return new WaitForSecondsRealtime(
                    interval
                );
            }
        }
    }

    public void PlayMoneyTick(
    float volumeScale = 1f)
    {
        if(moneySound == null)
        {
            Debug.LogWarning(
                "Money Sound가 연결되지 않았습니다."
            );

            return;
        }

        PlayVFX(
            moneySound,
            volumeScale
        );
    }
}