using UnityEngine;

public class Snack : MonoBehaviour
{
    public int scoreValue = 10;
    public SnackType type = SnackType.Basic;

    public enum SnackType { Basic, Rare }

    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        transform.position += Vector3.left * GameManager.Instance.CurrentSpeed * Time.deltaTime;

        if (transform.position.x < -15f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        GameManager.Instance.CollectSnack(scoreValue);
        gameObject.SetActive(false);
    }
}
