using UnityEngine;

public class PartCell : MonoBehaviour
{
    public PlacedPart Owner { get; private set; }
    public float collisionDamage;

    private void Awake()
    {
        Owner = GetComponentInParent<PlacedPart>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Owner.DecreaseHp(collisionDamage);
        }
    }
}
