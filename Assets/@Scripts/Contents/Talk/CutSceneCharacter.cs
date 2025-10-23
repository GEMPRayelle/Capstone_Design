using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 비주얼 노벨 캐릭터 개별 제어 클래스
/// 
/// 주요 역할:
/// - 캐릭터의 시각적 표현 관리 (스프라이트)
/// - 화면 내 이동 애니메이션 처리
/// - 상태 머신을 통한 행동 제어
/// - 고성능 스프라이트 검색 (Dictionary)
/// 
/// 사용 방식:
/// - EcCutsceneManager가 오브젝트 풀로 관리
/// - EcCutscene에서 각 대화마다 제어
/// </summary>
[Serializable] // Inspector에서 편집 가능하도록 직렬화
public class CutSceneCharacter : MonoBehaviour
{
    // ==================== 📊 상태 머신 ====================

    /// <summary>
    /// 캐릭터의 가능한 상태 정의
    /// Enum(열거형): 미리 정의된 상수 집합
    /// 
    /// 장점:
    /// - 코드 가독성 향상 (숫자 대신 의미 있는 이름)
    /// - 오타 방지 (컴파일 타임 체크)
    /// - switch 문에서 모든 케이스 검증 가능
    /// </summary>
    public enum CharacterState
    {
        /// <summary>
        /// 씬에 정지한 상태 (대기 중)
        /// - 이동 완료 후 상태
        /// - 아무 동작도 하지 않음
        /// </summary>
        StayInScene,

        /// <summary>
        /// 이동 중 상태
        /// - 목표 위치로 향해 움직이는 중
        /// - 매 프레임 위치 업데이트
        /// </summary>
        Moving
    }

    // ==================== 🎯 상태 변수 ====================

    [Header("State Settings")]
    [HideInInspector] // Inspector에 표시하지 않음 (런타임 중 자동 변경)
    [Tooltip("캐릭터의 현재 상태 (StayInScene, Moving)")]
    public CharacterState characterState;

    [Tooltip("캐릭터가 이동할 목표 위치")]
    // private로 선언하여 외부에서 직접 수정 불가
    // SetCharacterMove() 메서드를 통해서만 설정
    private Vector3 targetMovePosition;

    // ==================== 🎨 스프라이트 관리 ====================

    [Header("Sprite Settings")]
    [HideInInspector] // Inspector에 표시하지 않음 (자동 할당)
    [Tooltip("캐릭터 스프라이트를 표시하는 SpriteRenderer 컴포넌트")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("캐릭터가 사용할 스프라이트 배열 (표정/포즈)")]
    // Inspector에서 드래그 앤 드롭으로 설정
    // 예: sprite[0] = "happy", sprite[1] = "sad", sprite[2] = "angry"
    public Sprite[] spriteImages;

    [Tooltip("스프라이트 이름을 키로 하는 빠른 검색용 Dictionary")]
    // Dictionary<키, 값>: 해시테이블 기반 자료구조
    // 시간 복잡도: O(1) - 배열 O(n)보다 훨씬 빠름
    private Dictionary<string, Sprite> spriteDictionary;

    // ==================== 🚀 초기화 ====================

    /// <summary>
    /// Unity 생명주기: GameObject가 활성화될 때 호출
    /// 
    /// 실행 순서:
    /// 1. 스프라이트 Dictionary 생성
    /// 2. 배열의 모든 스프라이트를 Dictionary에 등록
    /// 3. SpriteRenderer 컴포넌트 참조 가져오기
    /// </summary>
    void Awake()
    {
        // ----- 1단계: Dictionary 초기화 -----
        // 빈 Dictionary 생성 (용량 자동 확장)
        spriteDictionary = new Dictionary<string, Sprite>();

        // ----- 2단계: 스프라이트 등록 -----
        // foreach: 배열의 모든 요소를 순회
        foreach (var sprite in spriteImages)
        {
            // sprite.name: Unity에서 자동으로 부여하는 스프라이트 이름
            // 예: "character_happy", "character_sad"
            // Dictionary에 [이름 → 스프라이트 객체] 매핑 저장
            spriteDictionary[sprite.name] = sprite;

            // 동일한 이름의 스프라이트가 있으면 덮어씀 (주의!)
        }

        // ----- 3단계: SpriteRenderer 참조 획득 -----
        // GetComponentInChildren: 자신과 자식 오브젝트에서 컴포넌트 검색
        // 캐릭터 구조:
        // - Character (이 스크립트가 붙은 GameObject)
        //   └─ SpriteObject (SpriteRenderer가 붙은 자식 GameObject)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // null 체크는 ChangeSpriteByName에서 수행
    }

    // ==================== 🎨 스프라이트 변경 ====================

    /// <summary>
    /// 이름으로 캐릭터의 스프라이트를 변경
    /// 
    /// 성능 비교:
    /// - 배열 검색 (O(n)): 최악의 경우 모든 요소 순회
    /// - Dictionary 검색 (O(1)): 해시 계산 후 즉시 접근
    /// 
    /// 사용 예시:
    /// character.ChangeSpriteByName("happy");     // 행복한 표정
    /// character.ChangeSpriteByName("sad");       // 슬픈 표정
    /// character.ChangeSpriteByName("surprised"); // 놀란 표정
    /// 
    /// TryGetValue 사용 이유:
    /// - ContainsKey + Dictionary[key]보다 효율적
    /// - 한 번의 해시 계산으로 존재 여부와 값 획득
    /// </summary>
    /// <param name="spriteName">변경할 스프라이트의 이름</param>
    public void ChangeSpriteByName(string spriteName)
    {
        // ----- null 체크: SpriteRenderer 존재 확인 -----
        if (spriteRenderer != null)
        {
            // ----- Dictionary에서 스프라이트 검색 -----
            // TryGetValue(키, out 변수):
            // - 키가 존재하면 true 반환 + 변수에 값 할당
            // - 키가 없으면 false 반환
            if (spriteDictionary.TryGetValue(spriteName, out var newSprite))
            {
                // ----- 스프라이트 변경 -----
                // SpriteRenderer의 sprite 속성에 새 스프라이트 할당
                // 화면에 즉시 반영됨
                spriteRenderer.sprite = newSprite;
            }
            else
            {
                // ----- 오류 처리: 스프라이트를 찾지 못한 경우 -----
                // 가능한 원인:
                // 1. 오타 (예: "hapy" 대신 "happy")
                // 2. Inspector에서 해당 스프라이트를 등록하지 않음
                // 3. 스프라이트 이름이 다름 (Asset 파일명 확인 필요)
                Debug.LogWarning($"Sprite with name {spriteName} not found.");
            }
        }
        else
        {
            // ----- 오류 처리: SpriteRenderer가 없는 경우 -----
            // 가능한 원인:
            // 1. 자식 GameObject에 SpriteRenderer 컴포넌트 없음
            // 2. Awake()가 실행되지 않음
            // 3. GameObject 구조가 잘못됨
            Debug.LogWarning("spriteRenderer is null.");
        }
    }

    // ==================== 🔄 상태 업데이트 ====================

    /// <summary>
    /// 캐릭터의 현재 상태를 체크하고 업데이트
    /// 
    /// 호출 시점:
    /// - EcCutscene의 Update()에서 매 프레임 호출
    /// - 모든 활성 캐릭터에 대해 반복 실행
    /// 
    /// 상태별 동작:
    /// - Moving: 목표 위치로 이동 (Lerp)
    /// - StayInScene: 아무 동작도 하지 않음
    /// 
    /// Vector3.MoveTowards 설명:
    /// - 부드러운 선형 이동 (Linear Interpolation)
    /// - 목표를 넘어가지 않음 (자동으로 멈춤)
    /// - 일정한 속도 유지
    /// </summary>
    public void CheckingCharacterState()
    {
        // ----- 이동 속도 계산 -----
        // step: 이번 프레임에 이동할 거리
        // characterTransitionSpeed: 초당 이동 거리 (단위/초)
        // Time.deltaTime: 이전 프레임과의 시간 차이 (초)
        // 
        // 예시:
        // - speed = 30, deltaTime = 0.016 (60fps)
        // - step = 30 * 0.016 = 0.48 유닛 이동
        var step = Managers.CutScene.characterTransitionSpeed * Time.deltaTime;

        // ----- 상태별 처리 -----
        // switch: enum 값에 따라 분기
        switch (characterState)
        {
            case CharacterState.Moving:
                // ----- 이동 중 상태 -----

                // Vector3.MoveTowards(현재 위치, 목표 위치, 최대 이동 거리)
                // 반환값: 새로운 위치 (목표를 넘지 않음)
                // 
                // 동작 방식:
                // 1. 현재 위치와 목표 위치 사이의 방향 계산
                // 2. step만큼 그 방향으로 이동
                // 3. 목표에 도달하면 정확히 목표 위치 반환
                transform.position = Vector3.MoveTowards(
                    transform.position,    // 현재 위치
                    targetMovePosition,    // 목표 위치
                    step                   // 이번 프레임 이동 거리
                );

                // ----- 도착 체크 -----
                // Vector3 비교: 세 축(x, y, z) 모두 일치하는지 확인
                if (targetMovePosition == transform.position)
                {
                    // 목표 위치에 도달 → 상태 전환
                    characterState = CharacterState.StayInScene;
                    Debug.Log("Play StayInScene");

                    // 이제 이동이 멈추고 대기 상태로 전환
                }

                break;

            case CharacterState.StayInScene:
                // ----- 대기 상태 -----
                // 아무 동작도 하지 않음 (빈 케이스)
                // break로 switch 문 종료
                break;
        }
    }

    // ==================== 🚶 이동 설정 ====================

    /// <summary>
    /// 캐릭터를 목표 위치로 이동 시작
    /// 
    /// 매개변수 설명:
    /// - targetPosition: 이동할 목표 위치 (Vector3)
    /// - targetRotation: 목표 회전 (현재 미사용)
    /// - targetScale: 목표 크기 (현재 미사용)
    /// 
    /// ⚠️ 주의: 회전과 크기 변환은 아직 구현되지 않음
    /// 
    /// 동작 방식:
    /// 1. 목표 위치 저장
    /// 2. 현재 위치와 다르면 Moving 상태로 전환
    /// 3. CheckingCharacterState()에서 실제 이동 처리
    /// 
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="targetRotation">목표 회전 (미구현)</param>
    /// <param name="targetScale">목표 크기 (미구현)</param>
    public void SetCharacterMove(Vector3 targetPosition, Vector3 targetRotation, Vector3 targetScale)
    {
        // ----- 목표 위치 저장 -----
        // private 변수에 저장하여 CheckingCharacterState에서 사용
        targetMovePosition = targetPosition;

        // ⚠️ targetRotation과 targetScale은 저장되지 않음 (미구현)

        // ----- 이동 필요 여부 확인 -----
        // 현재 위치와 목표 위치가 다른지 체크
        if (transform.position != targetMovePosition)
        {
            // 위치가 다르면 이동 상태로 전환
            characterState = CharacterState.Moving;
            Debug.Log("Play Moving");

            // CheckingCharacterState()에서 실제 이동 시작됨
        }

        // 현재 위치 == 목표 위치이면 아무 동작도 하지 않음
        // (이미 그 위치에 있으므로 이동 불필요)
    }
}
