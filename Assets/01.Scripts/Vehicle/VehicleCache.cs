using System.Collections.Generic;
using UnityEngine;

// 파츠 1개의 정보를 기억할 구조체
public struct PlacedPartSaveData
{
    public int PartKey;           // PartData의 고유 Key (Dictionary 검색용)
    public Vector2Int Origin;     // 기준 좌표
    public int Rotation;          // 회전값
}

// 씬 전환/스테이지 스왑 간 데이터를 쥐고 있을 정적 컨테이너
public static class VehicleCache
{
    public static bool HasSavedData = false;
    public static List<PlacedPartSaveData> SavedParts = new List<PlacedPartSaveData>();

    public static void Clear()
    {
        SavedParts.Clear();
        HasSavedData = false;
    }
}