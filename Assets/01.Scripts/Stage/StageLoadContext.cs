/// <summary>
/// 씬 전환 시 스테이지 정보를 안전하게 전달하기 위한 정적 데이터 컨텍스트입니다.
/// 새로운 씬의 StageManager가 데이터를 소비하는 구조로 동작합니다.
/// </summary>

public static class StageLoadContext
{
    private static int _stageIndex = -1;
    public static bool HasValue => _stageIndex != -1;

    public static void SetStageIndex(int index)
    {
        _stageIndex = index;
    }

    public static int GetStageIndex()
    {
        int value = HasValue ? _stageIndex : 0;
        _stageIndex = -1;
        return value;
    }
}