using UnityEngine;

public class CollisionTestDriver : MonoBehaviour
{
    [SerializeField] private RatController _attacker;
    [SerializeField] private RatController _target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_attacker == null)
            {
                Debug.LogError($"{name}: _attacker가 할당되지 않았습니다.");
                return;
            }

            if (_target == null)
            {
                Debug.LogError($"{name}: _target가 할당되지 않았습니다.");
                return;
            }

            bool result = _attacker.TryCollide(_target);
            Debug.Log($"충돌 시도 결과: {result}");
        }
    }
}