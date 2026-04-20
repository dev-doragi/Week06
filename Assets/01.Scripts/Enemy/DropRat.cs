using System.Collections;
using UnityEngine;

public class DropRat : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite _alive;
    [SerializeField] private Sprite _running;
    [SerializeField] private float _knockBackPower = 4f;
    [SerializeField] private float _moveSpeed = 1f;

    private bool move = false;
    private void OnEnable()
    {
        Vector2 explosionDir = new Vector2(Random.Range(-1f, -.2f), Random.Range(1.5f, .5f));

        rigid.AddForce(explosionDir * _knockBackPower, ForceMode2D.Impulse);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 15)
        {
            sr.sprite = _running;
            move = true;
        }
        if (collision.gameObject.GetComponentInParent<GridBoard>().boardOwner == GridBoard.BoardOwnerType.Player)
        {
            PlacementManager.Instance.AddMouseCount(2);
            PoolManager.Instance.Despawn(gameObject);
        }
    }

    private void Update()
    {
        if (move)
            rigid.linearVelocity = Vector2.left * _moveSpeed;
    }

}
