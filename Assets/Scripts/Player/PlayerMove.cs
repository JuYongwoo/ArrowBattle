using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    // SO 값으로 교체 예정

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = GetComponent<Player>().stat.Current.CurrentMoveSpeed; // Player 스크립트에서 이동 속도 가져오기
        sr = Util.getObjectInChildren(gameObject, "Cat").GetComponent<SpriteRenderer>(); // SpriteRenderer 캐싱
    }

    private void Update()
    {
        // 좌우 이동
        float moveX = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 좌우 플립
        if (moveX > 0.01f)        // 오른쪽 이동
            sr.flipX = false;
        else if (moveX < -0.01f)  // 왼쪽 이동
            sr.flipX = true;
    }
}
