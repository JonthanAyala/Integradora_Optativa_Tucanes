using UnityEngine;

public static class ScoreManager
{
    private static int score = 0;

    public static int Score => score;

    public static void AddPoints(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }
}
