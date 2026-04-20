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
        Debug.Log(Owner.gameObject.name);
        Debug.Log(Owner.PartData.CollisionPower);
        Owner.ApplyDirectDamage(Owner.PartData.CollisionPower);
    }
}
