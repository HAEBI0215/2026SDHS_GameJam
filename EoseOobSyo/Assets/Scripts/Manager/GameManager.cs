using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TimeManager Time { get; private set; }
    public SoundManager Sound { get; private set; }
    public UIManager UI { get; private set; }

    public enum GameState
    {
        Ready,
        Cooking,
        Result
    }

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;

        DontDestroyOnLoad(transform.root);

        Initialize();
    }

    private void Start()
    {
        StartGame();
    }

    private void Initialize()
    {
        Time = GetComponent<TimeManager>();
        Sound = GetComponent<SoundManager>();
        UI = GetComponent<UIManager>();

        Debug.Log("Manager Initialize");
    }

    public void StartGame()
    {
        CurrentState = GameState.Cooking;

        Time.ResetTimer();

        Debug.Log("장사 시작");
    }

    public void EndGame()
    {
        CurrentState = GameState.Result;

        Debug.Log("장사 종료");
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Cooking;
    }
}