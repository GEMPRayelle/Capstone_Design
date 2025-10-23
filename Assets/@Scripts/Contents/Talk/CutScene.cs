using System;
using UnityEngine;
using UnityEngine.Events;
using static CutScene;
using UnityEngine.TextCore.Text;
using Spine.Unity.Examples;
using UnityEngine.UI;

/// <summary>
/// 비주얼 노벨 스타일의 컷씬을 관리하는 메인 클래스
/// - 대화 텍스트의 타이핑 애니메이션
/// - 캐릭터의 등장, 이동, 표정 변경
/// - 배경 소품(Props)의 표시 및 페이드 효과
/// - 컷씬 자동/수동 진행 제어
/// </summary>
public class CutScene : MonoBehaviour
{
    /// <summary>
    /// 캐릭터 정보를 담는 데이터 클래스
    /// 각 컷씬에서 캐릭터가 어떻게 등장하고 움직일지 정의
    /// </summary>
    [Serializable]
    public class CharacterData 
    {
        [Tooltip("캐릭터의 이름.")]
        public string name;

        [Tooltip("캐릭터의 초기 위치/회전/크기 설정 ID")]
        public string initialTransformID;

        [Tooltip("캐릭터의 최종 위치/회전/크기 설정 ID")]
        public string finalTransformID;

        [Tooltip("캐릭터의 스프라이트 이름")]
        public string spriteString;
    }

    /// <summary>
    /// 배경 소품(Props) 정보를 담는 데이터 클래스
    /// 씬에 등장하는 오브젝트, 아이템, 배경 요소 등을 관리
    /// </summary>
    [Serializable]
    public class PropsData
    {
        [Tooltip("소품의 이름 (식별자)")]
        public string name;

        [Tooltip("씬에서 소품이 위치할 좌표")]
        public Vector3 position;

        [Tooltip("소품이 페이드 인되는 속도 (초 단위)")]
        // 값이 작을수록 빠르게 나타남
        public float fadeSpeed;
    }

    /// <summary>
    /// UnityEvent를 직렬화하기 위한 래퍼 클래스.
    /// Inspector에서 이벤트를 설정할 수 있도록 함
    /// </summary>
    [Serializable]
    public class CSUnityEvent : UnityEvent 
    {
        //TODO
        //-> 사운드 출력등 여러 이벤트 실행되어야함
    }

    //데이터 시트로 팔 데이터들
    /*
     * CharacterData class
     * PropData Class
     * name
     * nameString
     * ChatString
     * 
     */
    [Serializable]
    public class CutSceneData
    {
        [Header("Cutscene Data")]
        [Tooltip("컷씬의 이름 (디버깅용)")]
        public string name;

        [Tooltip("컷씬에 등장하는 캐릭터 배열")]
        public CharacterData[] charactersData;

        [Tooltip("컷씬에 표시할 소품들 배열")]
        public PropsData[] propsData;

        [Tooltip("대화에 표시될 캐릭터 이름")]
        public string nameString;

        [Tooltip("컷씬동안 대화창에 표시될 대화 내용")]
        [TextArea] public string chatString;

        [Header("Cutscene Event")]
        [Tooltip("컷씬 시작 전에 실행될 이벤트")]
        public CSUnityEvent cutscenePreEvent;

        [Tooltip("컷씬 종료 후에 실행될 이벤트")]
        public CSUnityEvent cutscenePostEvent;
    }


    // ==================== 🔧 컷씬 설정 ====================

    [SerializeField]
    [Tooltip("전체 컷씬 시퀀스 배열 (스토리 전체)")]
    // Inspector에서 설정할 컷씬들의 배열
    CutSceneData[] cutsceneData;

    [Tooltip("현재 재생 중인 컷씬의 인덱스")]
    // 0부터 시작하여 배열을 순차적으로 진행
    public int currentID;

    [Header("Cutscene Settings")]
    [HideInInspector] // Inspector에 표시하지 않음
    [Tooltip("자동 재생 타이머 (현재 남은 시간)")]
    // 설정된 시간 후 자동으로 다음 컷씬으로 진행
    float autoplayTime;

    // ==================== 🖼️ UI 참조 ====================

    [Header("Other Settings (no need to change)")]
    [SerializeField]
    [Tooltip("캐릭터 이름을 표시할 Text UI 컴포넌트")]
    Text charaNameText;

    [SerializeField]
    [Tooltip("대화 내용을 표시할 Text UI 컴포넌트")]
    Text chatText;

    // ==================== ⚙️ 내부 변수 ====================

    [Tooltip("현재 컷씬의 전체 대화 텍스트")]
    // 타이핑 애니메이션의 목표 문자열
    string chatTextString;

    [Tooltip("타이핑 애니메이션의 글자 간 딜레이 타이머")]
    // 한 글자를 출력하고 다음 글자까지의 대기 시간
    float typingTimer;

    [Tooltip("타이핑 애니메이션 진행 여부")]
    // true: 현재 글자를 출력 중, false: 완료됨
    bool startTyping;

    [Tooltip("현재 활성화된 소품 배열")]
    // 이전 컷씬의 소품을 제거하기 위해 추적
    PropsData[] activePropsData;

    // ==================== 🎬 초기화 ====================

    /// <summary>
    /// 게임 시작 시 호출되는 Unity 생명주기 함수
    /// </summary>
    void Start()
    {
        // 컷씬 시스템 초기화 및 첫 번째 컷씬 시작
        StartCutscene();
    }

    /// <summary>
    /// 컷씬 시스템을 초기화하고 첫 컷씬을 시작
    /// - 인덱스를 0으로 리셋
    /// - 타이머 및 상태 변수 초기화
    /// - 첫 번째 컷씬 재생
    /// </summary>
    public void StartCutscene()
    {
        // 첫 번째 컷씬부터 시작
        currentID = 0;

        // 전역 설정에서 자동 재생 시간 가져오기
        autoplayTime = Managers.CutScene.autoplayTime;

        // 타이핑 애니메이션 초기 상태는 비활성
        startTyping = false;

        // 전역 설정에서 타이핑 속도 가져오기
        typingTimer = Managers.CutScene.chatTypingDelay;

        // 첫 번째 컷씬 재생 시작
        PlayCutscene();
    }

    // ==================== 🔄 업데이트 루프 ====================

    /// <summary>
    /// 매 프레임 호출되는 Unity 생명주기 함수
    /// - 자동 재생 타이머 업데이트
    /// - 타이핑 애니메이션 진행
    /// - 캐릭터 상태 업데이트 (이동 애니메이션 등)
    /// - 소품 상태 업데이트 (페이드 애니메이션 등)
    /// </summary>
    void Update()
    {
        // 자동 재생 기능: 일정 시간 후 자동으로 다음 컷씬
        AutoPlayingCutscene();

        // 타이핑 애니메이션이 활성화되어 있으면 실행
        if (startTyping)
            StartTypingAnimation(chatText, chatTextString);

        // 현재 컷씬의 모든 캐릭터 상태 업데이트
        for (int i = 0; i < cutsceneData[currentID].charactersData.Length; i++)
        {
            // 캐릭터 이름으로 실제 캐릭터 오브젝트 가져오기
            string tempName = cutsceneData[currentID].charactersData[i].name;
            CutSceneCharacter character = Managers.CutScene.getCharacterObject(tempName);

            // 캐릭터의 상태 체크 (이동 완료 여부, 애니메이션 등)
            character.CheckingCharacterState();
        }

        // 현재 활성화된 소품이 있으면 업데이트
        if (activePropsData != null)
        {
            for (int i = 0; i < activePropsData.Length; i++)
            {
                // 소품 이름으로 실제 소품 오브젝트 가져오기
                CutSceneProps props = Managers.CutScene.getPropObject(activePropsData[i].name);

                // 소품의 페이드 애니메이션 등 업데이트
                props.PropUpdate();
            }
        }
    }

    // ==================== 🎭 컷씬 재생 ====================

    /// <summary>
    /// 현재 인덱스의 컷씬을 재생
    /// 실행 순서:
    /// 1. Pre 이벤트 호출
    /// 2. 대화 텍스트 타이핑 시작
    /// 3. 캐릭터 표시 및 애니메이션
    /// 4. 이전 소품 제거
    /// 5. 새로운 소품 표시
    /// </summary>
    void PlayCutscene()
    {
        // 1. 컷씬 시작 전 이벤트 실행
        InvokePreEvent();

        // 2. 대화 텍스트 타이핑 애니메이션 시작
        PlayChatTypingAnimation();

        // 3. 캐릭터들을 화면에 표시하고 애니메이션 적용
        ShowingCurrentCharacters();

        // 4. 이전 컷씬의 소품들 제거
        ClearPreviousProps();

        // 5. 현재 컷씬의 소품들 표시
        ShowingCurrentProps();
    }

    // ==================== 🎪 이벤트 시스템 ====================

    /// <summary>
    /// 현재 컷씬의 Pre 이벤트 호출
    /// Inspector에서 설정한 이벤트를 실행
    /// 예: 배경음악 변경, 화면 효과, 변수 설정 등
    /// </summary>
    void InvokePreEvent()
    {
        // null 체크: 이벤트가 설정되어 있는지 확인
        if (cutsceneData[currentID].cutscenePreEvent != null)
        {
            // Unity Event 시스템을 통해 이벤트 실행
            cutsceneData[currentID].cutscenePreEvent.Invoke();
        }
    }

    /// <summary>
    /// 현재 컷씬의 Post 이벤트 호출
    /// 컷씬이 완료된 후 실행
    /// 예: 아이템 획득, 플래그 설정, 다음 씬 로드 준비 등
    /// </summary>
    void InvokePostEvent()
    {
        // null 체크: 이벤트가 설정되어 있는지 확인
        if (cutsceneData[currentID].cutscenePostEvent != null)
        {
            // Unity Event 시스템을 통해 이벤트 실행
            cutsceneData[currentID].cutscenePostEvent.Invoke();
        }
    }

    // ==================== ⌨️ 타이핑 애니메이션 ====================

    /// <summary>
    /// 대화 텍스트의 타이핑 애니메이션 시작
    /// - UI 텍스트 초기화
    /// - 전체 텍스트 문자열 저장
    /// - 캐릭터 이름 표시
    /// </summary>
    void PlayChatTypingAnimation()
    {
        // 대화 텍스트 초기화 (빈 문자열로 시작)
        chatText.text = "";

        // 현재 컷씬의 전체 대화 내용 저장
        chatTextString = cutsceneData[currentID].chatString;

        // 타이핑 사운드 재생 (주석 처리됨)
        //CutsceneManager.instance.audioSource.Play();

        // 타이핑 애니메이션 시작
        startTyping = true;

        // 캐릭터 이름 표시
        charaNameText.text = cutsceneData[currentID].nameString;

        // 이름이 비어있으면 이름창 숨김, 있으면 표시
        if (charaNameText.text == "")
            charaNameText.transform.parent.gameObject.SetActive(false);
        else
            charaNameText.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// 타이핑 애니메이션 실행 (Update에서 매 프레임 호출)
    /// 한 글자씩 텍스트를 추가하여 타자기 효과 구현
    /// </summary>
    /// <param name="chatText">대화를 표시할 UI Text 컴포넌트</param>
    /// <param name="stringResult">출력할 전체 문자열</param>
    void StartTypingAnimation(Text chatText, string stringResult)
    {
        // 타이머 감소 (deltaTime: 이전 프레임과의 시간 차이)
        typingTimer -= Time.deltaTime;

        // 타이머가 0 이하가 되면 다음 글자 출력
        if (typingTimer <= 0)
        {
            // 아직 출력할 글자가 남아있는지 확인
            if (chatText.text != stringResult || chatText.text.Length < stringResult.Length)
            {
                // 다음 글자 하나 추가
                // stringResult[chatText.text.Length]: 현재 길이 인덱스의 글자
                chatText.text += stringResult[chatText.text.Length];

                // 타이머 리셋 (다음 글자를 위한 대기)
                typingTimer = Managers.CutScene.chatTypingDelay;
            }
            else
            {
                // 모든 글자 출력 완료, 타이핑 중지
                startTyping = false;
            }
        }
    }

    // ==================== 👥 캐릭터 관리 ====================

    /// <summary>
    /// 현재 컷씬의 캐릭터들을 화면에 표시하고 설정 적용
    /// - 캐릭터 활성화
    /// - 초기 위치/회전/크기 설정
    /// - 목표 위치로 이동 애니메이션 시작
    /// - 스프라이트(표정/포즈) 변경
    /// - 말하는 캐릭터 강조 (밝기 조절)
    /// </summary>
    void ShowingCurrentCharacters()
    {
        // 현재 컷씬의 모든 캐릭터 데이터를 순회
        for (int i = 0; i < cutsceneData[currentID].charactersData.Length; i++)
        {
            // 현재 처리 중인 캐릭터 데이터
            CharacterData tempCharaData = cutsceneData[currentID].charactersData[i];

            // 캐릭터 이름으로 실제 캐릭터 오브젝트 참조 가져오기
            CutSceneCharacter character = Managers.CutScene.getCharacterObject(tempCharaData.name);

            // null 체크: 캐릭터가 존재하는지 확인
            if (character != null)
            {
                // 캐릭터 오브젝트 활성화 (화면에 표시)
                character.transform.gameObject.SetActive(true);

                // ========== Transform 설정 (위치/회전/크기) ==========
                // 초기 Transform ID와 최종 Transform ID가 모두 설정되어 있는지 확인
                if (tempCharaData.initialTransformID != "" && tempCharaData.finalTransformID != "")
                {
                    // ID로 초기 Transform 설정 가져오기
                    TransformSetting charaInitialTransform
                        = Managers.CutScene.getCharaTransformSetting(tempCharaData.initialTransformID);

                    // ID로 최종 Transform 설정 가져오기
                    TransformSetting charaFinalTransform
                        = Managers.CutScene.getCharaTransformSetting(tempCharaData.finalTransformID);

                    // 초기 Transform이 존재하는지 확인
                    if (charaInitialTransform != null)
                    {
                        // ----- 초기 Transform 즉시 적용 -----
                        // 회전 설정 (Quaternion.Euler: 각도를 쿼터니언으로 변환)
                        character.transform.localRotation = Quaternion.Euler(charaInitialTransform.rotation);

                        // 크기 설정
                        character.transform.localScale = charaInitialTransform.scale;

                        // 위치 설정
                        character.transform.position = charaInitialTransform.position;

                        // ----- 최종 Transform으로 애니메이션 시작 -----
                        if (charaFinalTransform != null)
                        {
                            // 캐릭터가 initialTransform에서 finalTransform으로 이동
                            // 부드러운 애니메이션 효과 (Lerp 등 사용)
                            character.SetCharacterMove(
                                charaFinalTransform.position,
                                charaFinalTransform.rotation,
                                charaFinalTransform.scale
                            );
                        }
                    }
                    else
                    {
                        // 초기 Transform 설정을 찾을 수 없을 때 경고
                        Debug.Log("There are no transform setting named with " + tempCharaData.initialTransformID);
                    }
                }
                // ====================================================

                // ========== 스프라이트 변경 (표정/포즈) ==========
                // 스프라이트 이름이 설정되어 있으면 변경
                if (tempCharaData.spriteString != "")
                    character.ChangeSpriteByName(tempCharaData.spriteString);

                // ----- 말하는 캐릭터 강조 -----
                // 현재 대화하는 캐릭터는 밝게, 나머지는 어둡게
                if (character.name == cutsceneData[currentID].nameString)
                    character.spriteRenderer.color = Color.white; // 밝게 (말하는 캐릭터)
                else
                    character.spriteRenderer.color = Color.gray; // 어둡게 (듣는 캐릭터)
                                                                 // ================================================
            }
            else
            {
                // 캐릭터를 찾을 수 없을 때 경고
                Debug.Log("There are no characters named with " + tempCharaData.name);
            }
        }
    }

    // ==================== 🎨 소품(Props) 관리 ====================

    /// <summary>
    /// 이전 컷씬의 소품들을 화면에서 제거
    /// 컷씬이 전환될 때 이전 장면의 오브젝트를 정리
    /// </summary>
    void ClearPreviousProps()
    {
        // 이전에 활성화된 소품 배열이 존재하는지 확인
        if (activePropsData != null)
        {
            // 모든 이전 소품을 순회하며 비활성화
            for (int i = 0; i < activePropsData.Length; i++)
            {
                // 소품 이름으로 실제 오브젝트 참조 가져오기
                CutSceneProps props = Managers.CutScene.getPropObject(activePropsData[i].name);

                // null 체크: 소품이 존재하는지 확인
                if (props != null)
                {
                    // 소품 오브젝트 비활성화 (화면에서 숨김)
                    props.transform.gameObject.SetActive(false);
                }
                else
                {
                    // 소품을 찾을 수 없을 때 경고
                    Debug.Log("There are no properties named with " + activePropsData[i].name);
                }
            }
        }
    }

    /// <summary>
    /// 현재 컷씬의 소품들을 화면에 표시
    /// - 소품 활성화
    /// - 위치 설정
    /// - 페이드 인 애니메이션 시작
    /// </summary>
    void ShowingCurrentProps()
    {
        // 현재 컷씬의 소품 배열을 활성 소품으로 설정
        // (다음 컷씬에서 제거하기 위해 추적)
        activePropsData = cutsceneData[currentID].propsData;

        // 모든 소품을 순회하며 설정
        for (int i = 0; i < activePropsData.Length; i++)
        {
            // 소품 이름으로 실제 오브젝트 참조 가져오기
            CutSceneProps props = Managers.CutScene.getPropObject(activePropsData[i].name);

            // null 체크: 소품이 존재하는지 확인
            if (props != null)
            {
                // 소품 오브젝트 활성화 (화면에 표시)
                props.transform.gameObject.SetActive(true);

                // 소품 위치 설정
                props.transform.position = activePropsData[i].position;

                // 소품 크기를 0으로 초기화 (페이드 인 효과를 위해)
                props.transform.localScale = Vector3.zero;

                // 페이드 인 애니메이션 시작
                // fadeSpeed: 페이드 인 되는 속도
                props.SetVisibility(activePropsData[i].fadeSpeed);
            }
            else
            {
                // 소품을 찾을 수 없을 때 경고
                Debug.Log("There are no properties named with " + activePropsData[i].name);
            }
        }
    }

    // ==================== ⏭️ 컷씬 진행 제어 ====================

    /// <summary>
    /// 다음 컷씬으로 진행 (또는 타이핑 스킵)
    /// 클릭 이벤트나 버튼에 연결하여 사용
    /// 
    /// 동작 방식:
    /// 1. 타이핑 중이면 → 전체 텍스트 즉시 표시 (스킵)
    /// 2. 타이핑 완료 상태면 → 다음 컷씬으로 진행
    /// 3. 마지막 컷씬이면 → 컷씬 종료
    /// </summary>
    public void PlayNextCutscene()
    {
        // 타이핑이 완료되었는지 확인
        if (chatText.text == chatTextString)
        {
            // ----- 타이핑 완료 상태: 다음 컷씬으로 진행 -----

            // Post 이벤트 실행 (컷씬 종료 이벤트)
            InvokePostEvent();

            // 다음 컷씬이 존재하는지 확인
            if (currentID < cutsceneData.Length - 1)
            {
                // 인덱스 증가 (다음 컷씬으로)
                currentID += 1;

                // 자동 재생 타이머 리셋
                autoplayTime = Managers.CutScene.autoplayTime;

                // 다음 컷씬 재생
                PlayCutscene();
            }
            else
            {
                // ----- 마지막 컷씬: 시퀀스 종료 -----
                Debug.Log("Cutscene finished");

                // 컷씬 시스템 종료 (UI 숨김, 게임 재개 등)
                Managers.CutScene.closeCutscenes();
            }
        }
        else
        {
            // ----- 타이핑 중: 전체 텍스트 즉시 표시 (스킵) -----

            // 전체 텍스트를 한 번에 표시
            chatText.text = chatTextString;

            // 타이핑 애니메이션 중지
            startTyping = false;

            // 타이머 리셋
            typingTimer = 0;
        }
    }

    /// <summary>
    /// 자동 재생 기능 (Update에서 매 프레임 호출)
    /// 설정된 시간이 지나면 자동으로 다음 컷씬으로 진행
    /// - 타이핑이 완료된 후에만 작동
    /// - autoplayTime이 0 이상일 때만 활성화
    /// </summary>
    void AutoPlayingCutscene()
    {
        // 전역 설정의 자동 재생 시간 가져오기
        float temp = Managers.CutScene.autoplayTime;

        // 자동 재생이 활성화되어 있고 타이핑이 완료된 경우
        if (temp >= 0 && chatText.text == chatTextString)
        {
            // 자동 재생 타이머 감소
            autoplayTime -= Time.deltaTime;

            // 타이머가 0 이하가 되면 다음 컷씬으로
            if (autoplayTime <= 0)
            {
                PlayNextCutscene();
            }
        }
    }
}
