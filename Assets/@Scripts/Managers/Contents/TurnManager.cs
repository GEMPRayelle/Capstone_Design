using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class BattleInfo 
{
    //Creature가 CanUseSkillList에 담긴 스킬을 사용할 수 있다는 정보와 현재 Grid 위치를 담고있음
    public Creature CurrentCreature {  get; private set; } //현재 턴을 진행중인 Creature
    public List<SkillBase> CanUseSkillList { get; private set; }
    public Vector2Int CellPos { get; private set; }

    public BattleInfo(Creature Player, List<SkillBase> canUseSkillList)
    {
        CurrentCreature = Player;
        CanUseSkillList = canUseSkillList;
    }
}

public class TurnManager 
{
    public TurnSorting turnSorting; //턴 정렬 방식

    private int _currentTurn;
    public int CurrentTurn => _currentTurn;
    private List<BattleInfo> _battleInfoOrderList = new List<BattleInfo>(); //각 캐릭터 별로 BattleInfo를 저장

    //이벤트
    public Action<Player> startNewCreatureTurn; //새로운 캐릭터 턴 시작
    public Action<Player> turnOrderSet; //턴 순서 설정
    public Action<Player> turnPreviewSet; //턴 미리보기 설정

    public List<Creature> activePlayerList = new List<Creature>(); //Map상에서 배치된 플레이어 캐릭터들 리스트
    public List<Creature> activeMonsterList = new List<Creature>(); //Map상에서 배치된 플레이어 캐릭터들 리스트

    //턴 순서 미리 보기 리스트
    public List<TurnOrderPreviewObject> turnOrderPreview;
    public List<TurnOrderPreviewObject> currentTurnOrderPreview;

    //현재 턴을 진행중인 Creature
    public Creature activeCharacter;
    
    //전투 시작전 초기 전투 정보 초기화
    private void SetBattleInfoOrder()
    {
        _battleInfoOrderList.Clear(); //전투 정보 초기화
        activePlayerList.Clear(); //Map상의 플레이어들 초기화

        //전투 가능한 플레이어가 전부 다 살아있으면 아군팀에 추가
        foreach (Player player in Managers.Object.LivingPlayerList)
        {
            if (player.IsAlive)
            {
                _battleInfoOrderList.Add(new BattleInfo(player, player.GetUsableSkillList()));
                activePlayerList.Add(player);
            }
        }
        //턴을 진행할 Creature 리스트에 이동 거리에 따라 내림차순 정렬 진행
        //성능 최적화가 필요할시 LINQ대신 정렬 함수를 직접 구현할 것
        activePlayerList = activePlayerList
            .OrderByDescending(creature => creature.CreatureData.MovementRange)
            .ToList();

        //전투 가능한 몬스터가 전부 다 살아있으면 적팀에 추가
        foreach (Monster monster in Managers.Object.LivingMonsterList)
        {
            _battleInfoOrderList.Add(new BattleInfo(monster, monster.GetUsableSkillList()));
        }
        //적들도 턴을 진행할 우선순위에 따라 정렬이 필요
    }

    //전투 시작시 첫 Creature의 턴 진행
    public void StartTurn()
    {
        //살아있는 캐릭터들이 있는지 확인
        if(HasAliveCharacters())
        {
            //현재 전투에서 활성화된 플레이어들 중 턴을 진행할 캐릭터의 턴을 시작
            //_battleInfoDict[activePlayerList[0]].CurrentCreature.StartTurn();
            
            //이벤트 호출
        }

        
    }

    //턴 종료 후 다음 Creature의 턴 시작
    public void EndTurn()
    {
        if (turnOrderPreview.Count > 0)
        {
            FinalEndCharacterTurn();

            if (HasAliveCharacters())
            {
                if (activeCharacter.IsAlive)
                {

                }
                else
                {
                    EndTurn(); //죽었으면 다음 턴으로
                }
            }
        }

        //턴 종료시 타일 효과 적용 및 공격 수행을 위한 최종 리스트에 추가

    }

    //턴 순서 정렬
    private void SortingTurn(bool updateListSize = false)
    {
        var combinedList = new List<Creature>();

        //재 정렬이 필요하면
        if (updateListSize)
        {
            //살아있는 캐릭터들만 포함
            combinedList.AddRange(activePlayerList.Where(x => x.IsAlive));
            combinedList.AddRange(activeMonsterList.Where(x => x.IsAlive));

            turnOrderPreview = combinedList
                .OrderBy(x => x.CreatureData.MovementRange)
                .Select(x => new TurnOrderPreviewObject(x, 0)) //임시로 0
                .ToList();

            activeCharacter = turnOrderPreview[0].character;
            
        }
        //재 정렬이 필요 없으면
        else
        {
            // 턴 종료 후 순서 갱신
            TurnOrderPreviewObject item = turnOrderPreview[0];
            turnOrderPreview.RemoveAt(0);
            turnOrderPreview.Add(item);

            activeCharacter = turnOrderPreview[0].character;
        }

        //현재 턴 순서 저장 및 이벤트 트리거
        currentTurnOrderPreview = turnOrderPreview;
        //turnOrderSet. 이벤트 실행함수
    }

    // 살아있는 캐릭터가 있는지 확인
    private bool HasAliveCharacters() =>
        turnOrderPreview.Any(x => x.character.IsAlive);

    //턴 종료시 타일 효과 적용 및 이니셔티브 갱신
    private void FinalEndCharacterTurn()
    {
        //만약 타일에 효과 (맹독성 효과, 마그마 등의)가 생기면 턴 종료시 effect 효과를 주는 부분 추가해야함
        //if (activeCharacter.currentStandingTile && activeCharacter.currentStandingTile.tileData)
        //{
        //    var tileEffect = activeCharacter.activeTile.tileData.effect;
        //    if (tileEffect != null)
        //        activeCharacter.AttachEffect(tileEffect);
        //}
    }

}

//턴 순서를 미리 보기 위한 객체 클래스
public class TurnOrderPreviewObject
{
    public Creature character; // 해당 턴의 캐릭터
    public int PreviewInitiativeValue; // 이니셔티브 값 (턴 순서 결정 기준)

    public TurnOrderPreviewObject(Creature character, int previewInitiativeValue)
    {
        this.character = character;
        PreviewInitiativeValue = previewInitiativeValue;
    }
}
