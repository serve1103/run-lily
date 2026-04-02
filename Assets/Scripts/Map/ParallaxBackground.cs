using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float speedMultiplier = 0.5f; // 1.0 = 전경 속도, 0.1 = 원경(느림)

    [Header("Loop Settings")]
    public float spriteWidth = 20f; // 배경 스프라이트 너비
    public bool loop = true;

    float startX;

    void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        float speed = GameManager.Instance.CurrentSpeed * speedMultiplier;
        transform.position += Vector3.left * speed * Time.deltaTime;

        // 루프: 화면 밖으로 나가면 오른쪽으로 재배치
        if (loop && transform.position.x <= startX - spriteWidth)
        {
            transform.position = new Vector3(
                startX,
                transform.position.y,
                transform.position.z
            );
        }
    }
}
