using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class VehicleSaveLoadHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard _board;
    [SerializeField] private GridRenderer _gridRenderer;
    [Tooltip("BuildManager가 쓰는 부모 트랜스폼과 동일한 곳 연결")]
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

    // 스테이지 클리어 직후, 파괴되기 전에 호출
    public void SaveCurrentVehicle(int stageIndex)
    {
        VehicleCache.Clear();

        if (_placedPartsRoot == null) return;

        // 부모 객체 아래에 있는 모든 PlacedPart를 가져옴 (중복 없음)
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

        VehicleCache.HasSavedData = true;
        Debug.Log($"[SaveLoadHelper] 공성병기 파츠 {VehicleCache.SavedParts.Count}개 캐싱 완료.");
    }

    // 새 스테이지가 로드된 직후 호출
    public void LoadSavedVehicle(int stageIndex)
    {
        if (!VehicleCache.HasSavedData || VehicleCache.SavedParts.Count == 0) return;

        foreach (var savedData in VehicleCache.SavedParts)
        {
            // 1. GridManager에서 원본 데이터 찾기
            if (!GridManager.instance.partDic.TryGetValue(savedData.PartKey, out PartData data))
            {
                Debug.LogWarning($"[SaveLoadHelper] Key {savedData.PartKey}에 해당하는 파츠 데이터가 없습니다.");
                continue;
            }

            // 2. BuildManager.TryPlaceCurrentPart() 와 동일한 방식으로 파츠 생성
            GameObject partObj = new GameObject($"Placed_{data.partName}");
            partObj.transform.SetParent(_placedPartsRoot);

            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            // 3. 보드 규칙 검사 없이 무조건 배치 (이전 스테이지에서 이미 검증된 상태이므로)
            List<Vector2Int> targets = _board.GetRotatedCells(data, savedData.Origin, savedData.Rotation);
            placedPart.Initialize(data, savedData.Origin, savedData.Rotation, targets);

            foreach (var cell in targets)
            {
                _board.SetCell(cell, placedPart);
            }

            // 4. 시각적 표현 생성 (기존 BuildVisual 호출)
            placedPart.BuildVisual(_gridRenderer, placedPart.transform, Color.white);
        }

        Debug.Log("[SaveLoadHelper] 공성병기 캐싱 데이터 로드 및 재조립 완료.");
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Keyboard.current가 null인지 체크 (입력 장치 연결 확인)
        if (Keyboard.current == null) return;

        // 숫자 8번 키 (저장)
        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            Debug.Log("[Test] 8키 입력: 현재 배치 저장 시도");
            SaveCurrentVehicle(0);
        }

        // 숫자 9번 키 (로드)
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            Debug.Log("[Test] 9키 입력: 캐싱 데이터 로드 시도");
            LoadSavedVehicle(0);
        }
#endif
    }
}