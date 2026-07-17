using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get;
        private set;
    }

    public TimeManager Time
    {
        get;
        private set;
    }

    public SoundManager Sound
    {
        get;
        private set;
    }

    public UIManager UI
    {
        get;
        private set;
    }

    public OrderManager Order
    {
        get;
        private set;
    }

    public ScoreManager Score
    {
        get;
        private set;
    }

    public enum GameState
    {
        Ready,
        Cooking,
        Result
    }

    public GameState CurrentState
    {
        get;
        private set;
    }

    public event Action<GameState> OnGameStateChanged;

    private bool startGameAfterSceneReload;

    private void Awake()
    {
        if(Instance != null &&
        Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // GameManager는 씬과 함께 새로 생성돼야 함
        // DontDestroyOnLoad 사용하지 않음

        Initialize();
    }

    private void Start()
    {
        StartGame();
    }

    private void OnDestroy()
    {
        if(Time != null)
        {
            Time.OnTimeEnded -= EndGame;
        }

        if(Instance == this)
        {
            Instance = null;
        }
    }

    private void Initialize()
    {
        Time =
            GetComponent<TimeManager>();

        Sound =
            GetComponent<SoundManager>();

        UI =
            GetComponent<UIManager>();

        Order =
            GetComponent<OrderManager>();

        Score =
            GetComponent<ScoreManager>();

        Score?.Initialize(Order);

        if(Time != null)
        {
            Time.OnTimeEnded += EndGame;
        }

        SceneManager.sceneLoaded +=
            HandleSceneLoaded;

        CurrentState =
            GameState.Ready;

        Debug.Log("Manager Initialize");
    }

    public void StartGame()
    {
        UnityEngine.Time.timeScale = 1f;

        GameProgressManager.Instance?
            .BeginDay();

        Score?.ResetScore();

        Time?.ResetTimer();

        ShopUnlockManager.Instance?
            .RefreshUnlockState();

        Order?.StartOrderSystem();

        SetState(
            GameState.Cooking
        );

        int currentDay =
            GameProgressManager.Instance != null
                ? GameProgressManager.Instance.CurrentDay
                : 1;

        Debug.Log($"DAY {currentDay} 장사 시작");
    }

    public void EndGame()
    {
        if(CurrentState ==
           GameState.Result)
        {
            return;
        }

        Time?.StopTimer();

        Order?.StopOrderSystem();

        // 남아 있는 주문 영수증 정리
        Order?.ClearOrders();

        SettleCurrentDay();

        SetState(
            GameState.Result
        );

        // 결과 UI는 작동하고 실제 게임만 멈춤
        UnityEngine.Time.timeScale = 0f;

        Debug.Log("하루 영업 종료");
    }

    private void SettleCurrentDay()
    {
        if(GameProgressManager.Instance == null)
        {
            Debug.LogError(
                "GameProgressManager가 없습니다."
            );

            return;
        }

        if(Score == null)
        {
            Debug.LogError(
                "ScoreManager가 없습니다."
            );

            return;
        }

        GameProgressManager.Instance.SettleDay(
            Score.TotalEarnedMoney,
            Score.TotalExpiredPenalty,
            Score.TotalWrongSubmitPenalty,
            Score.CurrentScore,
            Score.CompletedOrderCount,
            Score.ExpiredOrderCount,
            Score.WrongSubmitCount
        );
    }

    public void StartNextDay()
    {
        if(CurrentState !=
           GameState.Result)
        {
            return;
        }

        UnityEngine.Time.timeScale = 1f;

        GameProgressManager.Instance?
            .AdvanceDay();

        startGameAfterSceneReload = true;

        SetState(
            GameState.Ready
        );

        int currentSceneIndex =
            SceneManager
                .GetActiveScene()
                .buildIndex;

        SceneManager.LoadScene(
            currentSceneIndex
        );
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if(!startGameAfterSceneReload)
            return;

        startGameAfterSceneReload = false;

        StartCoroutine(
            StartGameNextFrame()
        );
    }

    private IEnumerator StartGameNextFrame()
    {
        // 플레이어, 조리대, HUD의 Awake가 끝난 다음 시작
        yield return null;

        StartGame();
    }

    private void SetState(
        GameState newState)
    {
        CurrentState = newState;

        OnGameStateChanged?.Invoke(
            CurrentState
        );
    }

    public bool IsPlaying()
    {
        return CurrentState ==
               GameState.Cooking;
    }
}