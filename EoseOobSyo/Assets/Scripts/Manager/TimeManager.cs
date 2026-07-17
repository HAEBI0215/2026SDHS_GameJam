using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("영업 제한 시간")]
    [SerializeField]
    private float gameDuration = 240f;

    private float remainingTime;
    private bool isRunning;
    private bool isTimerPaused;

    public float RemainingTime =>
        remainingTime;

    public float GameDuration =>
        gameDuration;

    public bool IsRunning =>
        isRunning;

    public bool IsTimerPaused =>
        isTimerPaused;

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

        // 영업 제한시간만 정지
        // 플레이어, 조리, 주문 등은 계속 진행됨
        if(isTimerPaused)
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
        isTimerPaused = false;

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

        isTimerPaused = false;

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
        isTimerPaused = false;

        Debug.Log("영업 타이머 정지");
    }

    public void ResumeTimer()
    {
        if(remainingTime <= 0f)
            return;

        isRunning = true;
        isTimerPaused = false;

        Debug.Log("영업 타이머 재개");
    }

    public void SetTimerPaused(bool paused)
    {
        if(!isRunning)
        {
            isTimerPaused = false;
            return;
        }

        isTimerPaused = paused;

        Debug.Log(
            isTimerPaused
                ? "[TIME] 영업시간 일시정지"
                : "[TIME] 영업시간 재개"
        );
    }

    public void ToggleTimerPause()
    {
        SetTimerPaused(
            !isTimerPaused
        );
    }
}