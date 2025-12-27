using UnityEngine;

public static class GameResultData
{
    public static float TotalTime { get; private set; } = 0f;
    public static float TotalScore { get; private set; } = 0f;
    public static float Accuracy { get; private set; } = 0f;

    public static void SetResult(float totalTime, float totalScore, float accuracy)
    {
        TotalTime = totalTime;
        TotalScore = totalScore;
        Accuracy = accuracy;
    }

    public static void Reset()
    {
        TotalTime = 0f;
        TotalScore = 0f;
        Accuracy = 0f;
    }
}
