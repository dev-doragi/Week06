using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleSaveLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard _board;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private Transform _placedPartsRoot;

    [Header("Runtime Spawning")]
    [SerializeField] private PartRuntimeSpawner _partRuntimeSpawner;
    [SerializeField] private TeamType _teamType = TeamType.Player;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<StageClearedEvent>(OnStageCleared);
            EventBus.Instance.Subscribe<StageLoadedEvent>(OnStageLoaded);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<StageClearedEvent>(OnStageCleared);
            EventBus.Instance.Unsubscribe<StageLoadedEvent>(OnStageLoaded);
        }
    }

    /// <summary>
    /// [EventBus] 스테이지 클리어 시 차량 저장
    /// </summary>
    private void OnStageCleared(StageClearedEvent evt)
    {
        SaveCurrentVehicle(evt.StageIndex);
    }

    /// <summary>
    /// [EventBus] 스테이지 로드 시 차량 복구
    /// </summary>
    private void OnStageLoaded(StageLoadedEvent evt)
    {
        LoadSavedVehicle(evt.StageIndex);
    }

    public void SaveCurrentVehicle(int stageIndex)
    {
        Debug.Log($"[Vehicle] Stage {stageIndex} 저장 시작");
        VehicleCache.Clear();

        if (_placedPartsRoot == null)
        {
            Debug.LogError("Fail: _placedPartsRoot 누락");
            return;
        }

        // 현재 배치된 파츠 데이터 추출
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

            // RatStatRuntime HP 저장
            RatStatRuntime ratStat = part.GetComponentInChildren<RatStatRuntime>();
            if (ratStat != null)
            {
                VehicleCache.SavedRatStats[part.origin] = new RatStatSaveData
                {
                    Origin = part.origin,
                    CurrentHp = ratStat.CurrentHp
                };
            }
        }

        VehicleCache.HasSavedData = VehicleCache.SavedParts.Count > 0;
        Debug.Log($"Success: {VehicleCache.SavedParts.Count}개 캐싱 완료 (HP 캐싱: {VehicleCache.SavedRatStats.Count}개)");
    }

    public void LoadSavedVehicle(int stageIndex)
    {
        if (!VehicleCache.HasSavedData || VehicleCache.SavedParts.Count == 0)
        {
            Debug.Log("Skip: 복구할 데이터 없음");
            return;
        }

        // PartRuntimeSpawner 자동 탐색 (인스펙터 미연결 대비)
        if (_partRuntimeSpawner == null)
        {
            _partRuntimeSpawner = FindAnyObjectByType<PartRuntimeSpawner>();
            if (_partRuntimeSpawner == null)
                Debug.LogError("[VehicleSaveLoader] PartRuntimeSpawner를 씬에서 찾을 수 없습니다. 쥐 프리팹이 생성되지 않습니다.");
        }

        Debug.Log($"[Vehicle] Stage {stageIndex} 복구 시작");

        // 필드 초기화
        ClearCurrentField();

        int count = 0;
        foreach (var savedData in VehicleCache.SavedParts)
        {
            // 데이터 로드
            if (!GridManager.instance.partDic.TryGetValue(savedData.PartKey, out PartData data)) continue;

            // 오브젝트 생성 및 초기화
            GameObject partObj = new GameObject($"Placed_{data.PartName}");
            partObj.transform.SetParent(_placedPartsRoot);
            partObj.transform.position = _gridRenderer.GridToWorld(savedData.Origin);
            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            // 보드 데이터 등록
            List<Vector2Int> targets = _board.GetRotatedCells(data, savedData.Origin, savedData.Rotation);
            placedPart.Initialize(data, savedData.Origin, savedData.Rotation, targets);

            foreach (var cell in targets)
            {
                _board.SetCell(cell, placedPart);
            }

            // 비주얼 생성
            placedPart.BuildVisual(_gridRenderer, placedPart.transform, Color.white);

            // 런타임 프리팹 생성 (쥐 스프라이트/컴포넌트 포함)
            if (_partRuntimeSpawner != null)
            {
                _partRuntimeSpawner.SpawnRuntime(data, placedPart, _teamType, _board);
            }

            // RatStatRuntime HP 복구
            RatStatRuntime ratStat = placedPart.GetComponentInChildren<RatStatRuntime>();
            if (ratStat != null && VehicleCache.TryGetRatStat(savedData.Origin, out RatStatSaveData statData))
            {
                ratStat.InitializeStat();
                float damage = ratStat.MaxHp - statData.CurrentHp;
                if (damage > 0f)
                {
                    ratStat.ApplyDirectDamage(damage);
                }
                Debug.Log($"[RatStat] {data.PartName} HP 복구: {statData.CurrentHp}/{ratStat.MaxHp}");
            }

            count++;
        }

        Debug.Log($"Success: {count}개 복구 완료");
    }

    private void ClearCurrentField()
    {
        if (_placedPartsRoot == null || _board == null) return;

        int childCount = _placedPartsRoot.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = _placedPartsRoot.GetChild(i);
            if (child.TryGetComponent<PlacedPart>(out var part))
            {
                _board.RemovePart(part); // 보드 데이터 제거
            }
            Destroy(child.gameObject);
        }

        if (childCount > 0)
            Debug.Log($"CleanUp: 기존 객체 {childCount}개 제거");
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            StageManager.Instance.LoadNextStage();
        }

        // 테스트용 단축키 (8:저장, 9:로드)
        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            Debug.Log("[Test] Manual Save");
            SaveCurrentVehicle(0);
        }

        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            Debug.Log("[Test] Manual Load");
            LoadSavedVehicle(0);
        }
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            Debug.Log("<color=yellow>[Test] 스테이지 전환 시뮬레이션 시작</color>");

            if (StageManager.Instance != null)
            {
                int currentIndex = StageManager.Instance.CurrentStageIndex;
                StageManager.Instance.ClearCurrentStage();
                StageManager.Instance.LoadStage(currentIndex);

                Debug.Log("<color=cyan>[Test] 스테이지 전환 및 병기 복구 완료</color>");
            }
            else
            {
                Debug.LogError("StageManager 인스턴스를 찾을 수 없습니다.");
            }
        }
#endif
    }
}