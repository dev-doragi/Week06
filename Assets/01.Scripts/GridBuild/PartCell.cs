using UnityEngine;

public class PartCell : MonoBehaviour
{
    public RatController Owner { get; private set; }
    public float collisionDamage;

    private void Awake()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Owner = transform.parent.GetComponentInChildren<RatController>();
        if (Owner != null)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Owner.ApplyDirectDamage(collisionDamage);
        }
    }
}
