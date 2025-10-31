using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData")]
public class TileData : ScriptableObject
{
    #region Unity Tilemap 연동

    /// <summary>
    /// 이 TileData와 연결된 Unity TileBase들의 리스트
    /// 
    /// 용도:
    /// - Unity Tilemap에서 실제 사용되는 타일들과 게임 데이터 연결
    /// - 하나의 TileData가 여러 시각적 변형을 가질 수 있음
    /// 
    /// 예시:
    /// "잔디 타일" TileData에는:
    /// - 일반 잔디 타일
    /// - 꽃이 있는 잔디 타일  
    /// - 돌이 있는 잔디 타일
    /// 모두 같은 게임 속성(이동비용 1)을 공유하지만 시각적으로는 다름
    /// </summary>
    public List<TileBase> baseTiles;

    #endregion

    #region 툴팁 시스템

    /// <summary>
    /// 이 타일이 툴팁을 표시할지 여부를 결정하는 플래그
    /// 
    /// 활용 예시:
    /// - true: 특수 지형이나 함정 등 설명이 필요한 타일
    /// - false: 일반적인 평지 등 별도 설명이 불필요한 타일
    /// 
    /// UI 시스템에서 마우스 오버 시 툴팁 표시 여부를 결정
    /// </summary>
    public bool hasTooltip;

    //툴팁에 표시될 타일의 이름
    public string tooltipName;

    //툴팁에 표시될 상세 설명
    [TextArea(3, 10)]
    public string tooltipDescription;

    #endregion

    #region 게임 규칙 속성

    public TileTypes type = TileTypes.Traversable; // 타일의 기본 타입

    /// <summary>
    /// 이 타일을 지나가는데 필요한 이동 포인트
    /// 
    /// 값별 의미:
    /// - 1: 일반적인 평지, 도로 (기본값)
    /// - 2: 숲, 언덕 등 약간 어려운 지형
    /// - 3: 늪지대, 사막 등 매우 어려운 지형
    /// - 0: 특수한 경우 (순간이동 지점 등)
    /// 
    /// </summary>
    public int MoveCost = 1;

    // 이 타일에 적용되는 특수 효과
    public ScriptableEffect effect;

    #endregion
}

