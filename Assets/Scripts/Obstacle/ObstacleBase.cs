using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public float pushBackDistance = 2f; // 밀림 거리 (칸)
    public float recoveryTime = 1f;    // 회복 시간
    public AvoidMethod avoidMethod = AvoidMethod.Jump;

    public enum AvoidMethod { Jump, Slide, ChargeJump, Any }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // 월드가 왼쪽으로 이동 (릴리는 제자리)
        transform.position += Vector3.left * GameManager.Instance.CurrentSpeed * Time.deltaTime;

        // 화면 밖으로 나가면 비활성화 (풀링)
        if (transform.position.x < -15f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        // 슬라이딩으로 회피 가능한 장애물인데 슬라이딩 중이면 무시
        if (avoidMethod == AvoidMethod.Slide && player.IsSliding) return;

        // 밀림 발동
        player.TakePushBack(pushBackDistance, recoveryTime);
        gameObject.SetActive(false);
    }
}
