using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Won, Lost }

    public static GameManager Instance { get; private set; }

    [SerializeField] float timeLimit = 600f;   // 10 minutes
    [SerializeField] int quota = 20;           // incels that must be cleansed before time runs out

    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;
    public float TimeRemaining { get; private set; }
    public int CleansedCount { get; private set; }
    public int Quota => quota;
    public int Score { get; private set; }

    public event Action<GameState> GameEnded;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeRemaining = timeLimit;
    }

    void Update()
    {
        if (!IsPlaying) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            End(GameState.Lost);       // time ran out before the quota was met
        }
    }

    public void RegisterCleansed(int points)
    {
        if (!IsPlaying) return;

        CleansedCount++;
        Score += points;
        if (CleansedCount >= quota) End(GameState.Won);
    }

    void End(GameState result)
    {
        State = result;
        Time.timeScale = 0f;          // freeze the game; show your win/lose panel via GameEnded
        GameEnded?.Invoke(result);
        Debug.Log(result == GameState.Won ? "Quota met, you win!" : "Time's up, you lose!");
    }
}
