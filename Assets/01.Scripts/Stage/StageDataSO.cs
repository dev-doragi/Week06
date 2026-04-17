using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData_", menuName = "08.Data/Stage/StageData")]
public class StageDataSO : ScriptableObject
{
    [Header("Stage Layout (위치 껍데기)")]
    public StageLayout StageLayoutPrefab;

    [Header("Enemy Siege (적 공성병기)")]
    [Tooltip("이 스테이지에 1번 스폰될 적 공성병기 (풀링 안 함)")]
    public GameObject EnemySiegePrefab;

    [Header("Pool Settings (탄약/이펙트 풀링)")]
    [Tooltip("전투 중 계속 쏠 대포알, 이펙트 등만 등록하세요")]
    public List<PoolSetupData> PoolSetupList = new List<PoolSetupData>();

    [Header("Stage Info")]
    public int StageIndex;
    public float SpawnInterval; // 쫄따구 스폰 주기가 필요하다면 사용
}