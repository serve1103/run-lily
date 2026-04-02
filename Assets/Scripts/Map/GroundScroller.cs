using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    public float tileWidth = 10f;
    public Transform[] tiles; // 2~3개 타일을 이어 붙여 무한 스크롤

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        float speed = GameManager.Instance.CurrentSpeed;

        foreach (Transform tile in tiles)
        {
            tile.position += Vector3.left * speed * Time.deltaTime;

            // 화면 왼쪽 밖으로 나가면 오른쪽 끝으로 이동
            if (tile.position.x <= -tileWidth)
            {
                float rightMost = GetRightMostX();
                tile.position = new Vector3(
                    rightMost + tileWidth,
                    tile.position.y,
                    tile.position.z
                );
            }
        }
    }

    float GetRightMostX()
    {
        float max = float.MinValue;
        foreach (Transform tile in tiles)
        {
            if (tile.position.x > max) max = tile.position.x;
        }
        return max;
    }
}
