using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float returnSpeed = 3f;

    private float originPosX;
    private bool isReturning = false;
    private Rigidbody2D rigid;

    private void Start()
    {
        originPosX = transform.position.x;
        rigid = GetComponent<Rigidbody2D>();    
    }

    private void Update()
    {
        if (isReturning)
        {
            rigid.linearVelocity = Vector2.right * returnSpeed;

            if(transform.position.x >= originPosX)
            {
                isReturning = false;
            }
        }
        else
        {
            rigid.linearVelocity = Vector2.left * moveSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isReturning = true;
        }

    }
}
