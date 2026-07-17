using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("영업 제한 시간")]
    [SerializeField]
    private float gameDuration = 240f;

    private float remainingTime;
    private bool isRunning;

    public float RemainingTime =>
        remainingTime;

    public float GameDuration =>
        gameDuration;

    public bool IsRunning =>
        isRunning;

    public float RemainingRatio
    {
        get
        {
            if(gameDuration <= 0f)
                return 0f;

            return Mathf.Clamp01(
                remainingTime / gameDuration
            );
        }
    }

    public event Action<float> OnTimeChanged;
    public event Action OnTimeEnded;

    private void Update()
    {
        if(!isRunning)
            return;

        remainingTime -=
            UnityEngine.Time.deltaTime;

        remainingTime =
            Mathf.Max(
                0f,
                remainingTime
            );

        OnTimeChanged?.Invoke(
            remainingTime
        );

        if(remainingTime > 0f)
            return;

        isRunning = false;

        OnTimeEnded?.Invoke();

        Debug.Log("영업 시간 종료");
    }

    public void ResetTimer()
    {
        remainingTime =
            Mathf.Max(
                0f,
                gameDuration
            );

        isRunning =
            remainingTime > 0f;

        OnTimeChanged?.Invoke(
            remainingTime
        );

        Debug.Log(
            $"영업 타이머 시작: {gameDuration}초"
        );
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if(remainingTime <= 0f)
            return;

        isRunning = true;
    }
}