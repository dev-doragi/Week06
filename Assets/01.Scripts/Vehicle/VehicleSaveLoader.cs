using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class VehicleSaveLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard _board;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private Transform _placedPartsRoot;

    private void OnEnable()
    {
        StageManager.OnStageCleared += SaveCurrentVehicle;
        StageManager.OnStageLoaded += LoadSavedVehicle;
    }

    private void OnDisable()
    {
        StageManager.OnStageCleared -= SaveCurrentVehicle;
        StageManager.OnStageLoaded -= LoadSavedVehicle;
    }

    public void SaveCurrentVehicle(int stageIndex)
    {
        VehicleCache.Clear();
        if (_placedPartsRoot == null) return;

        PlacedPart[] currentParts = _placedPartsRoot.GetComponentsInChildren<PlacedPart>();
        foreach (var part in currentParts)
        {
            if (part.data == null) continue;

            VehicleCache.SavedParts.Add(new PlacedPartSaveData
            {
                PartKey = part.data.Key,
                Origin = part.origin,
                Rotation = part.rotation
            });
        }

        VehicleCache.HasSavedData = VehicleCache.SavedParts.Count > 0;
    }

    public void LoadSavedVehicle(int stageIndex)
    {
        if (!VehicleCache.HasSavedData || VehicleCache.SavedParts.Count == 0) return;

        // 중복 방지를 위한 사전 청소
        ClearCurrentField();

        foreach (var savedData in VehicleCache.SavedParts)
        {
            if (!GridManager.instance.partDic.TryGetValue(savedData.PartKey, out PartData data)) continue;

            GameObject partObj = new GameObject($"Placed_{data.partName}");
            partObj.transform.SetParent(_placedPartsRoot);
            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            List<Vector2Int> targets = _board.GetRotatedCells(data, savedData.Origin, savedData.Rotation);
            placedPart.Initialize(data, savedData.Origin, savedData.Rotation, targets);

            // 그리드 보드에 논리적 데이터 등록
            foreach (var cell in targets)
            {
                _board.SetCell(cell, placedPart);
            }

            placedPart.BuildVisual(_gridRenderer, placedPart.transform, Color.white);
        }
    }

    private void ClearCurrentField()
    {
        if (_placedPartsRoot == null || _board == null) return;

        // 하이어라키 역순 순회하여 오브젝트 및 그리드 데이터 제거
        for (int i = _placedPartsRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = _placedPartsRoot.GetChild(i);
            if (child.TryGetComponent<PlacedPart>(out var part))
            {
                _board.RemovePart(part); // GridBoard 내부 cells 배열 초기화
            }
            Destroy(child.gameObject);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit8Key.wasPressedThisFrame) SaveCurrentVehicle(0);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) LoadSavedVehicle(0);
#endif
    }
}