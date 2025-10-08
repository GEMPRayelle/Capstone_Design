using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

/// <summary>
/// 타일의 속성과 데이터를 정의하는 ScriptableObject 클래스
/// 
/// ScriptableObject의 장점:
/// 1. 메모리 효율성: 같은 타일 타입들이 하나의 데이터를 공유
/// 2. 에디터 통합: Unity Inspector에서 직접 편집 가능
/// 3. 에셋 관리: .asset 파일로 저장되어 버전 관리 용이
/// 4. 런타임 안정성: 게임 실행 중 데이터 변경되지 않음
/// 
/// Example:
/// - 잔디 타일: 이동 비용 1, 회복 효과
/// - 물 타일: 이동 불가, 익사 위험
/// - 용암 타일: 이동 비용 2, 화상 데미지
/// - 성벽 타일: 이동 불가, 방어력 보너스
/// </summary>
[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData")]
public class TileData : ScriptableObject
{
    public List<TileBase> baseTiles; //이 TileData와 연결된 Unity TileBase들의 리스트
    public bool hasToolTips; //이 타일이 툴팁을 표시할지 여부를 결정하는 플래그
    public string toolTipName; //툴팁에 표시될 타일의 이름

    /// <summary>
    /// 툴팁에 표시될 상세 설명
    /// TextArea 어트리뷰트로 에디터에서 여러 줄 편집 가능
    /// 
    /// Argument:
    /// - 3: 최소 줄 수 (에디터 UI 기본 크기)
    /// - 10: 최대 줄 수 (스크롤바 생성 기준)
    /// 
    /// 작성 예시:
    /// "독성 가스가 피어오르는 늪지대입니다.\n" +
    /// "이 지역에서 턴을 종료하면 독 상태에 걸립니다.\n" +
    /// "이동 비용: 2\n" +
    /// "효과: 턴 종료 시 독 데미지"
    /// </summary>
    [TextArea(3, 10)]
    public string toolTipDescription;
    public TileType type = TileType.Traversable; //타일의 기본 타입을 정의
    public int moveCost = 1; //이 타일을 지나가는데 필요한 이동 포인트
    public ScriptableEffect effect; //이 타일에 적용되는 특수 효과
}
