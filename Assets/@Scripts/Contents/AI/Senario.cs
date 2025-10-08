using UnityEngine;

/// <summary>
/// AI의 의사결정 시스템의 핵심 클래스
/// - 어디를 이동할지
/// - 누구를 공격할지
/// - 어떤 스킬을 쓸지
/// - 이 행동이 얼마나 유리할지 판단
/// </summary>
public class Senario
{
    public float senarioValue; //Score
    public SkillComponent targetSkill; //사용할 스킬
    public OverlayTile targetTile; //목표 타겟
    public OverlayTile positionTile; //이동할 위치
    public bool useNormalAttack; //기본 공격을 사용할지 여부

    public Senario(float senarioValue, SkillComponent targetSkill, OverlayTile targetTile, OverlayTile positionTile, bool useNormalAttack)
    {
        this.senarioValue = senarioValue;
        this.targetSkill = targetSkill;
        this.targetTile = targetTile;
        this.positionTile = positionTile;
        this.useNormalAttack = useNormalAttack;
    }

    public Senario()
    {
        this.senarioValue = -10000;
        this.targetSkill = null;
        this.targetTile = null;
        this.positionTile = null;
        this.useNormalAttack = false;
    }
}
