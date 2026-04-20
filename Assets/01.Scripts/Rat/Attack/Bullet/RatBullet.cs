using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RatBullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite _alive;
    [SerializeField] private Sprite _dead;
    [SerializeField] private float _knockBackPower = 2f;
    [SerializeField] private float _moveSpeed = 3f;

    private bool move = false;

    private void OnEnable()
    {
        Vector2 explosionDir= new Vector2(Random.Range(-1f, -.2f), Random.Range(1.5f, .5f));

        rigid.AddForce(explosionDir * _knockBackPower, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 15)
        {
            sr.sprite = _dead;
            move = true;
            StartCoroutine(Despawn());
        }
    }
    private void Update()
    {
        if (move)
            rigid.linearVelocity = Vector2.left * _moveSpeed;
    }


    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(1f);
        float elapsedTime = 0f;
        Color color = sr.color;

        while (elapsedTime < 1)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / 1);
            sr.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        sr.color = color;
        sr.sprite = _alive;
        move = false;
        // 이미 비활성화된 상태에서 중복 호출되는 것을 방지
        if (gameObject.activeInHierarchy)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
