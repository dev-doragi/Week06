using UnityEngine;

public class PartCell : MonoBehaviour
{
    RatController Owner;

    private void Awake()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Owner = transform.parent.GetComponentInChildren<RatController>();
        if (Owner == null)
            return;
        Owner.ApplyDirectDamage(Owner.PartData.CollisionPower);
    }
}
