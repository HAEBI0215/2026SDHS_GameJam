using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public enum PlayerType
    {
        Player1,
        Player2
    }

    [Header("플레이어 설정")]
    [SerializeField]
    private PlayerType playerType;


    [Header("이동")]
    [SerializeField]
    private float moveSpeed = 5f;


    private Rigidbody2D rb;

    private Vector2 movement;

    private Vector2 lastDirection = Vector2.down;


    public Vector2 Movement => movement;
    public Vector2 LastDirection => lastDirection;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        InputMove();
    }


    private void FixedUpdate()
    {
        Move();
    }


    private void InputMove()
    {
        movement = Vector2.zero;


        if(playerType == PlayerType.Player1)
        {
            if(Input.GetKey(KeyCode.W))
                movement.y += 1;

            if(Input.GetKey(KeyCode.S))
                movement.y -= 1;

            if(Input.GetKey(KeyCode.A))
                movement.x -= 1;

            if(Input.GetKey(KeyCode.D))
                movement.x += 1;
        }


        if(playerType == PlayerType.Player2)
        {
            if(Input.GetKey(KeyCode.UpArrow))
                movement.y += 1;

            if(Input.GetKey(KeyCode.DownArrow))
                movement.y -= 1;

            if(Input.GetKey(KeyCode.LeftArrow))
                movement.x -= 1;

            if(Input.GetKey(KeyCode.RightArrow))
                movement.x += 1;
        }


        movement = movement.normalized;


        if(movement != Vector2.zero)
        {
            UpdateLastDirection();
        }
    }


    private void UpdateLastDirection()
    {
        if(Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            lastDirection = new Vector2(
                Mathf.Sign(movement.x),
                0
            );
        }
        else
        {
            lastDirection = new Vector2(
                0,
                Mathf.Sign(movement.y)
            );
        }
    }


    private void Move()
    {
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}