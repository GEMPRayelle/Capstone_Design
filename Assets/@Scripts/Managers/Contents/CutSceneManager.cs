using UnityEngine;

/// <summary>
/// 비주얼 노벨 컷씬 시스템의 중앙 관리자 (싱글톤)
/// 
/// 주요 역할:
/// - 모든 캐릭터, 소품, 컷씬의 생명주기 관리
/// - 오브젝트 풀링으로 성능 최적화
/// - 전역 설정 값 제공 (타이핑 속도, 자동 재생 등)
/// - 다른 스크립트에서 접근 가능한 중앙 인터페이스
/// </summary>
public class CutSceneManager 
{
    // ==================== 👥 캐릭터 관리 ====================

    [Header("Character Settings")]
    [Tooltip("컷씬에서 사용할 캐릭터 프리팹 배열")]
    [SerializeField]
    GameObject[] characterPrefabs;

    [HideInInspector] // Inspector에 표시하지 않음 (자동 생성되는 값)
    [Tooltip("초기화된 캐릭터 오브젝트 배열 (인스턴스)")]
    // characterPrefabs를 실제로 생성(Instantiate)한 결과물
    CutSceneCharacter[] characters;

    // ==================== 🎨 소품(Props) 관리 ====================

    [Header("Properties Settings")]
    [Tooltip("컷씬에서 사용할 소품 프리팹 배열")]
    [SerializeField]
    GameObject[] propPrefabs;

    [HideInInspector] // Inspector에 표시하지 않음
    [Tooltip("초기화된 소품 오브젝트 배열 (인스턴스)")]
    // propPrefabs를 실제로 생성(Instantiate)한 결과물
    CutSceneProps[] props;

    // ==================== 🎬 컷씬 관리 ====================

    [Header("Cutscene Settings")]
    [Tooltip("관리할 컷씬 배열 (여러 장면의 대화 시퀀스)")]
    [SerializeField]
    CutScene[] cutscenes;

    [Tooltip("현재 활성화된 컷씬의 이름")]
    [SerializeField]
    string currentCutscene;

    [Tooltip("캐릭터 위치/회전/크기 프리셋 배열")]
    // 예: "left_start", "center", "right_end" 등의 사전 정의된 Transform 설정
    [SerializeField]
    TransformSetting[] transformSettings;

    // ==================== ⚙️ 전역 설정 ====================

    [Header("Other Settings")]
    [Tooltip("캐릭터, 소품 등 GUI 요소를 담는 부모 패널")]
    // 모든 비주얼 요소가 이 패널 아래에 생성됨
    [SerializeField]
    GameObject guiPanel;

    [Tooltip("자동 재생 타이머 (초). 0으로 설정하면 자동 재생 비활성화")]
    // 대화가 끝나고 자동으로 다음 대화로 넘어가는 시간
    public float autoplayTime;

    [Tooltip("캐릭터가 위치/회전/크기를 전환하는 속도. 기본값: 30")]
    // 값이 클수록 빠르게 이동/회전/크기 변경
    public float characterTransitionSpeed;

    [Tooltip("대화 타이핑 시 각 글자 간 딜레이 (초)")]
    // 값이 작을수록 빠르게 타이핑됨
    public float chatTypingDelay;

    // ==================== 🚀 초기화 ====================

    /// <summary>
    /// Unity 생명주기: Start보다 먼저 실행되는 초기화 함수
    /// 게임 오브젝트가 활성화될 때 가장 먼저 호출됨
    /// </summary>
    public void Init()
    {

        // 2. 모든 캐릭터 프리팹을 인스턴스화하여 오브젝트 풀 생성
        InitCharacters();

        // 3. 모든 소품 프리팹을 인스턴스화하여 오브젝트 풀 생성
        InitProps();

        // 4. 지정된 컷씬으로 시작
        InitCutscenes(currentCutscene);
    }

    // ==================== 👤 캐릭터 초기화 ====================

    /// <summary>
    /// 모든 캐릭터 프리팹을 인스턴스화하여 오브젝트 풀 생성
    /// 
    /// 오브젝트 풀링 이점:
    /// - 게임 중 매번 생성/삭제하지 않아 성능 향상
    /// - 필요할 때 활성화/비활성화만 하면 됨
    /// - 메모리 할당/해제로 인한 가비지 컬렉션 감소
    /// </summary>
    void InitCharacters()
    {
        // 프리팹 개수만큼 배열 생성
        characters = new CutSceneCharacter[characterPrefabs.Length];

        // 모든 캐릭터 프리팹 순회
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            // ----- 프리팹 인스턴스화 -----
            // Instantiate: Unity에서 오브젝트를 복제하여 씬에 생성
            //GameObject temp = Instantiate(characterPrefabs[i].gameObject.name);
            GameObject temp = Managers.Resource.Load<GameObject>(characterPrefabs[i].gameObject.name);

            // ----- 이름 정리 -----
            // Unity의 Instantiate는 자동으로 "(Clone)" 접미사를 추가함
            // 예: "Hero(Clone)" → "Hero"
            // 검색을 쉽게 하기 위해 "(Clone)" 제거
            temp.name = temp.name.Replace("(Clone)", "");

            // ----- 계층 구조 설정 -----
            // GUI 패널의 자식으로 설정하여 구조 정리
            // 장점: 패널을 끄면 모든 캐릭터도 함께 꺼짐
            temp.transform.SetParent(guiPanel.transform);

            // ----- 컴포넌트 참조 저장 -----
            // GameObject에서 EcCharacter 스크립트 컴포넌트 가져오기
            characters[i] = temp.GetComponent<CutSceneCharacter>();

            // 초기 상태: 비활성화 (필요할 때만 활성화)
            // (명시되지 않았지만 보통 초기에는 SetActive(false) 처리)
        }
    }

    // ==================== 🎨 소품 초기화 ====================

    /// <summary>
    /// 모든 소품 프리팹을 인스턴스화하여 오브젝트 풀 생성
    /// 캐릭터 초기화와 동일한 패턴
    /// </summary>
    void InitProps()
    {
        // 프리팹 개수만큼 배열 생성
        props = new CutSceneProps[propPrefabs.Length];

        // 모든 소품 프리팹 순회
        for (int i = 0; i < propPrefabs.Length; i++)
        {
            // ----- 프리팹 인스턴스화 -----
            //GameObject temp = Instantiate(propPrefabs[i]);
            GameObject temp = Managers.Resource.Load<GameObject>(propPrefabs[i].gameObject.name);

            // ----- 이름 정리 -----
            // "(Clone)" 접미사 제거
            temp.name = temp.name.Replace("(Clone)", "");

            // ----- 계층 구조 설정 -----
            // GUI 패널의 자식으로 설정
            temp.transform.SetParent(guiPanel.transform);

            // ----- 컴포넌트 참조 저장 -----
            props[i] = temp.GetComponent<CutSceneProps>();
        }
    }

    // ==================== 🎬 컷씬 제어 ====================

    /// <summary>
    /// 모든 컷씬을 종료하고 GUI를 숨김
    /// 
    /// 사용 시나리오:
    /// - 컷씬이 끝나고 게임플레이로 돌아갈 때
    /// - 다른 컷씬으로 전환하기 전 정리 작업
    /// - 사용자가 Skip 버튼을 눌렀을 때
    /// </summary>
    public void closeCutscenes()
    {
        // ----- GUI 패널 전체 비활성화 -----
        // 패널을 끄면 하위의 모든 캐릭터, 소품도 함께 꺼짐
        guiPanel.SetActive(false);

        // ----- 모든 컷씬 오브젝트 비활성화 -----
        for (int i = 0; i < cutscenes.Length; i++)
        {
            // 각 컷씬의 GameObject를 개별적으로 비활성화
            // Update 등의 로직이 더 이상 실행되지 않음
            cutscenes[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 지정된 이름의 컷씬을 초기화하고 시작
    /// 
    /// 실행 순서:
    /// 1. 현재 모든 컷씬 종료
    /// 2. 새로운 컷씬을 현재 컷씬으로 설정
    /// 3. 해당 컷씬 활성화 및 시작
    /// 
    /// 사용 예시:
    /// InitCutscenes("Chapter1_Opening");
    /// InitCutscenes("BossEncounter");
    /// </summary>
    /// <param name="cutsceneName">시작할 컷씬의 이름</param>
    public void InitCutscenes(string cutsceneName)
    {
        // ----- 1단계: 기존 컷씬 정리 -----
        // 모든 컷씬을 닫아서 깨끗한 상태로 만듦
        closeCutscenes();

        // ----- 2단계: 현재 컷씬 이름 저장 -----
        // 나중에 이 컷씬을 참조할 수 있도록 저장
        currentCutscene = cutsceneName;

        // ----- 3단계: 컷씬 오브젝트 검색 -----
        // 이름으로 해당 컷씬 오브젝트 찾기
        CutScene temp = getCutscenesObject(currentCutscene);

        // ----- 4단계: null 체크 및 실행 -----
        if (temp != null)
        {
            // GUI 패널 활성화 (캐릭터, 소품을 볼 수 있도록)
            guiPanel.SetActive(true);

            // 해당 컷씬 오브젝트 활성화
            temp.gameObject.SetActive(true);

            // 컷씬 시작 (첫 번째 대화부터 재생)
            temp.StartCutscene();
        }
        // temp가 null이면 getCutscenesObject 내부에서 경고 메시지 출력됨
    }

    // ==================== 🔍 검색 시스템 ====================

    /// <summary>
    /// 이름으로 캐릭터 오브젝트를 검색하여 반환
    /// 
    /// 선형 검색 (Linear Search):
    /// - 배열을 처음부터 끝까지 순회하며 일치하는 이름 찾기
    /// - 시간 복잡도: O(n)
    /// - 캐릭터 수가 많지 않으면 충분히 빠름
    /// 
    /// 사용 예시:
    /// EcCharacter hero = instance.getCharacterObject("Hero");
    /// hero.ChangeSpriteByName("happy");
    /// </summary>
    /// <param name="name">찾을 캐릭터의 이름</param>
    /// <returns>찾은 캐릭터 오브젝트, 없으면 null</returns>
    public CutSceneCharacter getCharacterObject(string name)
    {
        // 모든 캐릭터 배열 순회
        for (int i = 0; i < characters.Length; i++)
        {
            // 이름 비교 (대소문자 구분)
            if (name == characters[i].name)
                return characters[i]; // 찾으면 즉시 반환
        }

        // 배열 끝까지 찾지 못한 경우 null 반환
        // 호출하는 쪽에서 null 체크 필요
        return null;
    }

    /// <summary>
    /// 이름으로 소품 오브젝트를 검색하여 반환
    /// getCharacterObject와 동일한 패턴
    /// 
    /// 사용 예시:
    /// EcProps desk = instance.getPropObject("Desk");
    /// desk.SetVisibility(1.5f);
    /// </summary>
    /// <param name="name">찾을 소품의 이름</param>
    /// <returns>찾은 소품 오브젝트, 없으면 null</returns>
    public CutSceneProps getPropObject(string name)
    {
        // 모든 소품 배열 순회
        for (int i = 0; i < props.Length; i++)
        {
            // 이름 비교
            if (name == props[i].name)
                return props[i];
        }

        // 찾지 못한 경우 null 반환
        return null;
    }

    /// <summary>
    /// 이름으로 캐릭터 Transform 설정을 검색하여 반환
    /// 
    /// Transform 설정이란?
    /// - 사전 정의된 위치/회전/크기 프리셋
    /// - 예: "left_start" = (x:-5, y:0, z:0)
    /// - 컷씬 데이터에서 이 ID를 참조하여 캐릭터 배치
    /// 
    /// 사용 예시:
    /// EcTransformSetting leftPos = instance.getCharaTransformSetting("left_start");
    /// character.transform.position = leftPos.position;
    /// </summary>
    /// <param name="name">찾을 Transform 설정의 이름</param>
    /// <returns>찾은 Transform 설정, 없으면 null</returns>
    public TransformSetting getCharaTransformSetting(string name)
    {
        // 모든 Transform 설정 배열 순회
        for (int i = 0; i < transformSettings.Length; i++)
        {
            // 이름 비교
            if (name == transformSettings[i].name)
                return transformSettings[i];
        }

        // 찾지 못한 경우 null 반환
        return null;
    }

    /// <summary>
    /// 이름으로 컷씬 오브젝트를 검색하여 반환
    /// 
    /// 다른 검색 함수와 차이점:
    /// - 찾지 못했을 때 Debug 로그 출력
    /// - 컷씬을 찾지 못하는 것은 치명적인 오류이므로 명시적 경고
    /// 
    /// 사용 예시:
    /// EcCutscene chapter1 = instance.getCutscenesObject("Chapter1");
    /// chapter1.PlayNextCutscene();
    /// </summary>
    /// <param name="name">찾을 컷씬의 이름</param>
    /// <returns>찾은 컷씬 오브젝트, 없으면 null</returns>
    public CutScene getCutscenesObject(string name)
    {
        // 모든 컷씬 배열 순회
        for (int i = 0; i < cutscenes.Length; i++)
        {
            // 이름 비교
            if (name == cutscenes[i].name)
                return cutscenes[i];
        }

        // ----- 오류 처리 -----
        // 찾지 못한 경우 경고 로그 출력
        // Inspector 설정 오류나 오타를 디버깅하는 데 도움
        Debug.Log("cutscene with name " + name + " not found");

        return null;
    }

    // ==================== 🎮 외부 인터페이스 ====================

    /// <summary>
    /// 현재 활성화된 컷씬의 다음 대화로 진행
    /// 
    /// 사용 시나리오:
    /// - UI 버튼에 연결하여 클릭으로 진행
    /// - 입력 시스템에서 호출 (스페이스바, 마우스 클릭)
    /// - 외부 스크립트에서 프로그래밍 방식으로 진행
    /// 
    /// 동작 방식:
    /// 1. 현재 컷씬 오브젝트 가져오기
    /// 2. 해당 컷씬의 PlayNextCutscene() 메서드 호출
    /// 3. 내부적으로 타이핑 스킵 또는 다음 대화 진행
    /// 
    /// 사용 예시:
    /// // 버튼 클릭 이벤트
    /// public void OnNextButtonClick()
    /// {
    ///     EcCutsceneManager.instance.PlayNextCutscene();
    /// }
    /// 
    /// // 키보드 입력
    /// void Update()
    /// {
    ///     if (Input.GetKeyDown(KeyCode.Space))
    ///         EcCutsceneManager.instance.PlayNextCutscene();
    /// }
    /// </summary>
    public void PlayNextCutscene()
    {
        // 현재 컷씬 이름으로 오브젝트 가져와서 진행 메서드 호출
        // null 체크는 getCutscenesObject 내부에서 처리됨
        getCutscenesObject(currentCutscene).PlayNextCutscene();
    }
}

