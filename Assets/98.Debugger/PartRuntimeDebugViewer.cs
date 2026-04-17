using UnityEngine;

public class PartRuntimeDebugViewer : MonoBehaviour
{
    private RatController _ratController;

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatController를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        if (_ratController == null || _ratController.PartData == null)
        {
            return;
        }

        Debug.Log(
            $"{name} | " +
            $"PartName: {_ratController.PartData.PartName}, " +
            $"PartType: {_ratController.PartData.PartType}, " +
            $"CanUseAttack: {_ratController.CanUseAttack()}, " +
            $"CanUseCollision: {_ratController.CanUseCollision()}, " +
            $"IsArcAttack: {_ratController.IsArcAttack()}, " +
            $"IsDirectAttack: {_ratController.IsDirectAttack()}, " +
            $"IsAreaAttack: {_ratController.IsAreaAttack()}"
        );
    }
}