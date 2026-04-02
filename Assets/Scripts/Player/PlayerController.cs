using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float chargeJumpForce = 15f;
    public float chargeMinTime = 0.3f;
    public float chargeMaxTime = 0.8f;

    [Header("Slide Settings")]
    public float slideDuration = 2f;

    [Header("Push Back Settings")]
    public float pushBackSpeed = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Colliders")]
    public BoxCollider2D normalCollider;

    Rigidbody2D rb;
    bool isGrounded;
    bool isSliding;
    bool isCharging;
    bool isPushedBack;
    float chargeStartTime;
    float slideTimer;
    float pushBackAmount; // 누적 밀림 거리
    float pushBackRemaining; // 현재 밀려야 할 거리
    float baseXPosition; // 기본 x 위치

    // 슬라이딩 시 콜라이더 크기 변경용
    Vector2 normalColliderSize;
    Vector2 normalColliderOffset;
    Vector3 normalScale;

    // 입력 버퍼링
    bool bufferedJump;
    bool bufferedSlide;
    float inputBufferTime = 0.1f;
    float bufferTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseXPosition = transform.position.x;

        // 원래 콜라이더 크기 저장
        if (normalCollider)
        {
            normalColliderSize = normalCollider.size;
            normalColliderOffset = normalCollider.offset;
        }
        normalScale = transform.localScale;
    }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        CheckGround();
        HandleInput();
        UpdateSlide();
        UpdatePushBack();
        ProcessInputBuffer();
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleInput()
    {
        // 밀림 중에는 입력 무시
        if (isPushedBack) return;

        // 모바일: 화면 좌/우 터치 영역
        // PC: 좌클릭 = 점프, 우클릭 = 슬라이딩 (테스트용)
        bool leftDown = false, leftUp = false, rightDown = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        leftDown = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        leftUp = Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0);
        rightDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1);
#else
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            bool isLeftSide = touch.position.x < Screen.width / 2f;

            if (isLeftSide)
            {
                if (touch.phase == TouchPhase.Began) leftDown = true;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) leftUp = true;
            }
            else
            {
                if (touch.phase == TouchPhase.Began) rightDown = true;
            }
        }
#endif

        // 슬라이딩 중 점프 입력 → 즉시 캔슬 후 점프
        if (leftDown && isSliding)
        {
            CancelSlide();
            Jump(jumpForce);
            return;
        }

        // 왼쪽: 점프 / 차지 점프
        if (leftDown)
        {
            if (isGrounded)
            {
                isCharging = true;
                chargeStartTime = Time.time;
            }
            else
            {
                // 공중이면 버퍼링
                bufferedJump = true;
                bufferTimer = inputBufferTime;
            }
        }

        if (leftUp && isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            if (chargeTime >= chargeMinTime)
            {
                float t = Mathf.InverseLerp(chargeMinTime, chargeMaxTime, chargeTime);
                float force = Mathf.Lerp(jumpForce, chargeJumpForce, t);
                Jump(force);
            }
            else
            {
                Jump(jumpForce);
            }
            isCharging = false;
        }

        // 오른쪽: 슬라이딩
        if (rightDown)
        {
            if (isGrounded && !isSliding)
            {
                StartSlide();
            }
            else
            {
                bufferedSlide = true;
                bufferTimer = inputBufferTime;
            }
        }
    }

    void ProcessInputBuffer()
    {
        if (!isGrounded || isPushedBack) return;

        if (bufferedJump)
        {
            Jump(jumpForce);
            bufferedJump = false;
            bufferedSlide = false;
            return;
        }

        if (bufferedSlide)
        {
            StartSlide();
            bufferedSlide = false;
        }

        bufferTimer -= Time.deltaTime;
        if (bufferTimer <= 0f)
        {
            bufferedJump = false;
            bufferedSlide = false;
        }
    }

    void Jump(float force)
    {
        if (!isGrounded) return;
        CancelSlide();
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        // 콜라이더를 납작하게 (높이 절반, 아래로 이동)
        if (normalCollider)
        {
            normalCollider.size = new Vector2(normalColliderSize.x, normalColliderSize.y * 0.4f);
            normalCollider.offset = new Vector2(normalColliderOffset.x, normalColliderOffset.y - normalColliderSize.y * 0.3f);
        }
        // 스프라이트도 납작하게
        transform.localScale = new Vector3(normalScale.x * 1.3f, normalScale.y * 0.4f, normalScale.z);
    }

    void CancelSlide()
    {
        if (!isSliding) return;
        isSliding = false;

        // 원래 크기로 복구
        if (normalCollider)
        {
            normalCollider.size = normalColliderSize;
            normalCollider.offset = normalColliderOffset;
        }
        transform.localScale = normalScale;
    }

    void UpdateSlide()
    {
        if (!isSliding) return;
        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            CancelSlide();
        }
    }

    // 장애물에 부딪혔을 때 호출
    public void TakePushBack(float distance, float recoveryTime)
    {
        if (isPushedBack) return;

        isPushedBack = true;
        isCharging = false;
        CancelSlide();

        pushBackRemaining = distance;
        pushBackAmount += distance;

        // 밀림 누적이 한계 초과 → 게임오버
        if (pushBackAmount >= GameManager.Instance.gameOverPushBack)
        {
            GameManager.Instance.TriggerGameOver();
            return;
        }

        Invoke(nameof(EndPushBack), recoveryTime);
    }

    void UpdatePushBack()
    {
        if (!isPushedBack || pushBackRemaining <= 0f) return;

        float move = pushBackSpeed * Time.deltaTime;
        if (move > pushBackRemaining) move = pushBackRemaining;

        transform.position += Vector3.left * move;
        pushBackRemaining -= move;
    }

    void EndPushBack()
    {
        isPushedBack = false;
    }

    // 게임 속도에 맞춰 자연스럽게 원래 위치로 복귀
    void LateUpdate()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;
        if (isPushedBack) return;

        // 밀린 후 천천히 원래 위치로 복귀
        if (transform.position.x < baseXPosition)
        {
            float recovery = GameManager.Instance.CurrentSpeed * 0.3f * Time.deltaTime;
            float newX = Mathf.Min(transform.position.x + recovery, baseXPosition);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // 복귀 완료 시 밀림 누적 감소
            if (Mathf.Approximately(newX, baseXPosition))
            {
                pushBackAmount = Mathf.Max(0, pushBackAmount - 1f);
            }
        }
    }

    public bool IsSliding => isSliding;
    public bool IsGrounded => isGrounded;
    public bool IsPushedBack => isPushedBack;
    public float PushBackAmount => pushBackAmount;

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
