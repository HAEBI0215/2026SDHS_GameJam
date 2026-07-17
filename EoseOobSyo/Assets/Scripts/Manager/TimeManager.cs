using UnityEngine;


public class TimeManager : MonoBehaviour
{

    private float currentTime;

    [SerializeField]
    private float shopTime = 180f;

    public float CurrentTime
    {
        get { return currentTime; }
    }

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if(!GameManager.Instance.IsPlaying())
            return;

        currentTime -= Time.deltaTime;

        if(currentTime <= 0)
        {
            currentTime = 0;

            GameManager.Instance.EndGame();
        }
    }

    public void ResetTimer()
    {
        currentTime = shopTime;
    }
}