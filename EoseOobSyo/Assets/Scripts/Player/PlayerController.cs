using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("플레이어 세팅")]
    [SerializeField]
    private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private Vector2 movement;

    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        InputMove();
        RotatePlayer();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void InputMove()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;

        if(movement != Vector2.zero)
        {
            lastDirection = movement;
        }
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void RotatePlayer()
    {
        if(lastDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if(lastDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}