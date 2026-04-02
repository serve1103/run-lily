using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public Text scoreText;
    public Text distanceText;
    public Text snackText;

    [Header("Push Back Warning")]
    public Image pushBackWarning; // 왼쪽 빨간 테두리

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text finalDistanceText;
    public Text finalSnackText;
    public Button retryButton;
    public Button startButton;

    [Header("Start Panel")]
    public GameObject startPanel;

    PlayerController player;

    void Start()
    {
        gameOverPanel.SetActive(false);
        if (startPanel) startPanel.SetActive(true);

        GameManager.Instance.OnGameStart.AddListener(OnGameStart);
        GameManager.Instance.OnGameOver.AddListener(OnGameOver);
        GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
        GameManager.Instance.OnSnackCollected.AddListener(UpdateSnack);

        if (retryButton) retryButton.onClick.AddListener(Retry);
        if (startButton) startButton.onClick.AddListener(StartGame);

        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // 거리 표시
        if (distanceText)
            distanceText.text = $"{GameManager.Instance.Distance:F0}m";

        // 밀림 경고 (밀림 누적량에 따라 빨간 테두리 강도)
        if (pushBackWarning && player)
        {
            float ratio = player.PushBackAmount / GameManager.Instance.gameOverPushBack;
            Color c = pushBackWarning.color;
            c.a = ratio * 0.8f;
            pushBackWarning.color = c;
        }
    }

    void OnGameStart()
    {
        if (startPanel) startPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        UpdateScore(0);
        UpdateSnack(0);
    }

    void OnGameOver()
    {
        gameOverPanel.SetActive(true);
        if (finalScoreText) finalScoreText.text = $"점수: {GameManager.Instance.Score}";
        if (finalDistanceText) finalDistanceText.text = $"거리: {GameManager.Instance.Distance:F0}m";
        if (finalSnackText) finalSnackText.text = $"간식: {GameManager.Instance.SnackCount}개";
    }

    void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
    }

    void UpdateSnack(int count)
    {
        if (snackText) snackText.text = $"{count}/50";
    }

    void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
