using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
    }
    public void SubtractScore(int amount)
    {
        CurrentScore -= amount;
    }
    public void ResetScore()
    {
        CurrentScore = 0;
    }
}
