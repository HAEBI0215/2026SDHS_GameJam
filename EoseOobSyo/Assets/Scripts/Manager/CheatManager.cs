using UnityEngine;

public class CheatManager : MonoBehaviour
{
    private bool isCheatEnabled;
    private bool isTimeFrozen;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backslash))
        {
            ToggleCheat();
        }

        if(!isCheatEnabled)
            return;

        if(Input.GetKeyDown(KeyCode.Alpha1) ||
           Input.GetKeyDown(KeyCode.Keypad1))
        {
            ToggleTimeFreeze();
        }

        if(Input.GetKeyDown(KeyCode.Alpha2) ||
           Input.GetKeyDown(KeyCode.Keypad2))
        {
            AddMoney();
        }

        if(Input.GetKeyDown(KeyCode.Alpha3) ||
           Input.GetKeyDown(KeyCode.Keypad3))
        {
            EndDay();
        }
    }

    private void ToggleCheat()
    {
        isCheatEnabled = !isCheatEnabled;

        if(isCheatEnabled)
        {
            Debug.Log(
                "[CHEAT ON]\n" +
                "1: 시간 정지/재개\n" +
                "2: 돈 10,000원 추가\n" +
                "3: 하루 즉시 마무리"
            );
        }
        else
        {
            // 치트 모드를 끌 때 정지 상태도 해제
            if(isTimeFrozen)
            {
                isTimeFrozen = false;
                Time.timeScale = 1f;
            }

            Debug.Log("[CHEAT OFF]");
        }
    }

    private void ToggleTimeFreeze()
    {
        if(GameManager.Instance == null)
        {
            Debug.LogError(
                "[CHEAT] GameManager가 없습니다."
            );

            return;
        }

        if(GameManager.Instance.CurrentState !=
        GameManager.GameState.Cooking)
        {
            Debug.LogWarning(
                "[CHEAT] 영업 중에만 시간을 정지할 수 있습니다."
            );

            return;
        }

        TimeManager timeManager =
            GameManager.Instance.Time;

        if(timeManager == null)
        {
            Debug.LogError(
                "[CHEAT] TimeManager가 없습니다."
            );

            return;
        }

        timeManager.ToggleTimerPause();

        Debug.Log(
            timeManager.IsTimerPaused
                ? "[CHEAT] 영업시간만 정지"
                : "[CHEAT] 영업시간 재개"
        );
    }

    private void AddMoney()
    {
        if(GameProgressManager.Instance == null)
        {
            Debug.LogError(
                "[CHEAT] GameProgressManager가 없습니다."
            );

            return;
        }

        GameProgressManager.Instance.AddMoney(10000);
    }

    private void EndDay()
    {
        if(GameManager.Instance == null)
        {
            Debug.LogError(
                "[CHEAT] GameManager가 없습니다."
            );

            return;
        }

        if(GameManager.Instance.CurrentState ==
           GameManager.GameState.Result)
        {
            Debug.LogWarning(
                "[CHEAT] 이미 하루가 종료되었습니다."
            );

            return;
        }

        // 정지 상태에서 정산 UI가 꼬이지 않도록 먼저 복구
        isTimeFrozen = false;
        Time.timeScale = 1f;

        GameManager.Instance.EndGame();

        Debug.Log("[CHEAT] 하루 즉시 마무리");
    }

    private void OnDisable()
    {
        if(!isTimeFrozen)
            return;

        isTimeFrozen = false;
        Time.timeScale = 1f;
    }
}