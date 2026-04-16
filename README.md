## **0. 핵심 지침 (Core Principles)**
* **MainScene 보호**: 현재 핵심 시스템 구현 단계이므로 `MainScene`은 수정하지 않고 비워둡니다.
* **개인 작업 환경**: 각 작업자는 자신의 이름으로 된 전용 씬을 생성하여 테스트를 진행합니다. (예: `JaeinScene`)
* **씬 관리**: 모든 씬 파일(.unity)은 `00.Scenes` 폴더 내에 저장합니다.

## **1. 폴더 구조 (Folder Structure)**
* **00.Scenes**: 모든 씬 파일
* **01.Scripts**: 모든 C# 스크립트
* **02.Prefabs**: 재사용 가능한 프리팹
* **03.Art**: 3D 모델, 텍스처, 메테리얼
* **04.UI**: UI 이미지 및 프리팹
* **05.Audio**: SFX 및 BGM
* **06.VFX**: 이펙트 및 파티클 시스템
* **07.Data**: ScriptableObject, JSON, CSV
* **08.Shaders**: 커스텀 셰이더 및 그래프

## **2. Git 브랜치 전략 (GitFlow)**
* **main**: 최종 배포 및 빌드용 브랜치.
* **dev**: 개발 통합 브랜치. 모든 기능 구현 결과가 모이는 중심.
* **feature/**: 단위 기능 구현 브랜치. (예: `feature/player-meld`)

## **3. 코드 컨벤션 (C# Naming Convention)**
* **PascalCase**: 클래스(Class), 메서드(Method), 프로퍼티(Property)
* **_camelCase**: `private` 필드(Field). 접두어 언더바(`_`) 사용 필수.
* **camelCase**: 지역 변수(Local Variable), 파라미터(Parameter)

## **4. 프로그래밍 규칙 (Programming Rules)**

### **싱글톤 및 매니저 참조 규칙 (Singleton & Registry)**
* **최우선 초기화**: 모든 싱글톤 클래스 상단에 `[DefaultExecutionOrder(-100)]`을 선언하여 초기화 우선순위를 최상위로 올립니다.
* **인스턴스 관리**: `Awake`에서 `Instance` 할당, `DontDestroyOnLoad` 설정, `ManagerRegistry` 등록을 모두 완료합니다.
* **참조 방식**: 타 매니저 참조 시 `Instance` 직접 접근보다 `ManagerRegistry.Get<T>()` 또는 `TryGet<T>()` 사용을 권장합니다.
* **이벤트 생명주기**: 이벤트 구독은 `OnEnable`, 해제는 `OnDisable`에서 수행하여 메모리 누수를 방지합니다.

### **엄격한 예외 처리 (Strict Null Check)**
* **에러 로그 강제**: 참조 확인 시 단순 `return` 처리를 금지합니다. 반드시 `Debug.LogError()`를 호출하여 콘솔에 에러를 명시하고 로직을 중단합니다.

## **5. 코드 예시 (Code Example)**

```csharp
// Singleton.cs (통합 완전체)
[DefaultExecutionOrder(-100)]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool _isDontDestroyOnLoad = true;
    private static T _instance;
    public static T Instance 
    {
        get {
            if (_instance == null) Debug.LogError($"{typeof(T).Name} Instance is Null");
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
        if (_isDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        
        ManagerRegistry.Register<T>(_instance);
        OnInitialized();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    protected virtual void OnInitialized() { }
}

// 참조 및 사용 예시
public class PlayerController : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        if (ManagerRegistry.TryGet<GameManager>(out var manager))
        {
            _gameManager = manager;
        }
        else
        {
            Debug.LogError("GameManager를 찾을 수 없습니다.");
        }
    }

    private void OnEnable()
    {
        if (_gameManager != null) _gameManager.OnStateChanged += HandleState;
    }

    private void OnDisable()
    {
        if (_gameManager != null) _gameManager.OnStateChanged -= HandleState;
    }
}
```

## **6. 작업 흐름 요약**
1. `dev`에서 `feature/기능-이름` 브랜치 생성.
2. 개인용 테스트 씬에서 기능 구현.
3. 머지 전 `dev`를 자신의 브랜치로 선 병합(Pre-merge)하여 충돌 해결.
4. 컴파일 에러가 없는 상태로 `dev`에 병합.
