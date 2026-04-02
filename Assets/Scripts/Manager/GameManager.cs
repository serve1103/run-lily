using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float baseSpeed = 5f;
    public float maxSpeed = 12f;
    public float speedIncreasePerMeter = 0.01f; // 10m마다 1% = 0.01 per meter * baseSpeed

    [Header("Push Back Settings")]
    public float gameOverPushBack = 8f; // 8칸 밀리면 게임오버

    public enum GameState { Ready, Playing, Paused, GameOver }
    public GameState State { get; private set; } = GameState.Ready;

    public float CurrentSpeed { get; private set; }
    public float Distance { get; private set; }
    public int Score { get; private set; }
    public int SnackCount { get; private set; }

    public UnityEvent OnGameStart;
    public UnityEvent OnGameOver;
    public UnityEvent<int> OnScoreChanged;
    public UnityEvent<int> OnSnackCollected;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        // 거리 누적
        Distance += CurrentSpeed * Time.deltaTime;

        // 속도 점진적 증가
        CurrentSpeed = Mathf.Min(
            baseSpeed + (Distance * speedIncreasePerMeter),
            maxSpeed
        );
    }

    public void StartGame()
    {
        State = GameState.Playing;
        CurrentSpeed = baseSpeed;
        Distance = 0f;
        Score = 0;
        SnackCount = 0;
        OnGameStart?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        CurrentSpeed = 0f;
        OnGameOver?.Invoke();
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void CollectSnack(int scoreValue)
    {
        SnackCount++;
        AddScore(scoreValue);
        OnSnackCollected?.Invoke(SnackCount);
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
    }
}
