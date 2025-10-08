using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

/*
 * 턴 베이스 설계 리스트
 * {플레이어는 유저를 뜻하며} {캐릭터는 게임오브젝트를 뜻함}
 * 1. 플레이어는 게임 시작시 오더를 기준으로 3x3 quad에 스폰위치가 설정됨
 * 2. 그 위치에 캐릭터들을 스폰시키고 플레이어부터 턴이 시작됨
 * 3. 스폰된 각 캐릭터중에 한명을 선택하면 이동 거리를(불투명한 하늘색) 보여줌 
 * 4. 캐릭터의 현 currentPos에서 Monster를 공격하기위해 어느 위치로 이동해야 하는지를 보라색으로 표시 (Path가 아닌 CellPos)
 * 5. 캐릭터가 사용가능한 스킬 UI (버튼, 아이콘)등은 우측 하단에 위치 -> 스킬 사용시 이미지를 회색으로 변경
 * 6. 스킬은 캐릭터가 이동하기 전/후 어느 상황에서든 사용 가능, 이동 전에 스킬을 사용하더라도 후에 이동은 가능
 * 6-1. 특정 캐릭터는 스킬 자체가 후에 이동을 제한하는 경우는 있음 (ex: 저격수: 스킬 사용후 2턴동안 자리 고정)
 * 7. 턴 종료 버튼을 통해서 플레이어의 턴을 종료함 (플레이어턴에만 활성화)
 * 7-1. 턴이 종료될 시 플레이어들은 지정된 공격 (NormalAttack이나 Abillity를 사용함)
 * 7-2. 추가 턴을 받은 캐릭터는 추가 턴을 생성 후 캐릭터의 행동을 강제로 진행하게 하고 턴을 종료 시킴
 * 8. 플레이어 턴이 종료시 적 턴으로 이동
 * 
 * ------------------------------------
 * 캐릭터들은 턴 시작시 자유롭게 선택해서 이동시킬 수 있지만
 * 한번 이동이 끝난 캐릭터는 턴이 끝난다 activePlayerList에 등록된
 * 캐릭터들의 행동이 모두 끝나면 실질적인 턴 종료가 일어나며 지정된 공격이나 스킬 행동을 취한다
*/

public class TurnManager 
{
    public TurnSorting turnSorting; //턴 정렬 방식
    public TurnPhase CurrentPhase { get; private set; } //플레이어의 턴 상태 관리

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

    //추가 턴을 받는 캐릭터들
    public Queue<Creature> extraTurnPlayerQueue = new Queue<Creature>();

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

    //턴 종료 버튼 클릭시 일괄적으로 플레이어 턴 종료
    public void OnPlayerTurnEnd()
    {
        if (CurrentPhase != TurnPhase.PlayerMovement || !IsAllPlayerMoved())
            return;

        CurrentPhase = TurnPhase.PlayerAction;

        foreach (var player in activePlayerList)
        {
            if (player.IsAlive)
            {
                //NormalAttack or Skill
                player.CreatureState = ECreatureState.Skill;
            }
        }

        // Need delay?
        StartEnemyTurn();
    }

    //아직 행동하지 않은 Monster(한 마리) 턴 시작
    public void StartEnemyTurn()
    {
        if (IsAllMonsterMoved() == true) // 모든 적 행동 끝날 시
        {
            // 다시 플레이어 턴 시작 
            PrepareNextPlayerTurn();
        }

        else // 아직 행동이 남은 적이 있다면
        {
            // 살아있고 아직 이동하지 않은 첫 번째 몬스터 찾기
            Monster currentMonster = activeMonsterList
                .FirstOrDefault(m => m.IsAlive && m.IsMoved == false) as Monster;

            if (currentMonster != null)
            {
                activeCharacter = currentMonster;
                // AI 행동 로직 시작 함수
            }
        }
           
    }

    //턴 종료시 추가 턴 처리
    public void StartExtraTurn() 
    {
        while (extraTurnPlayerQueue.Count > 0) 
        {
            //추가 턴을 진행할 Creature를 큐에서 가져옴
            Creature extraTurnCreature = extraTurnPlayerQueue.Dequeue();

            activeCharacter = extraTurnCreature;
            CurrentPhase = TurnPhase.PlayerMovement;

            //UI 및 이벤트 처리

            //캐릭터가 Auto Action을 수행하거나 수동 입력을 기다림
        }
    }

    //추가 턴 종료
    public void EndExtraTurn()
    {
        FinalEndCharacterTurn();

        if (extraTurnPlayerQueue.Count > 0)
        {
            //추가 턴을 진행할 캐릭터가 더 있을때
            StartExtraTurn();
        }
        else
        {
            EndTurn();
        }
    }

    //다음 턴 준비
    private void PrepareNextPlayerTurn()
    {
        //모든 캐릭터의 이동 상태 초기화후 턴 넘김
        foreach (var player in activePlayerList)
        {
            player.IsMoved = false;
        }

        CurrentPhase = TurnPhase.PlayerMovement;
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

    //모든 캐릭터가 이동을 완료했는지 체크
    private bool IsAllPlayerMoved()
    {
        //TODO -> 이 함수 기반으로 턴 종료 버튼 Active 여부 체크
        return activePlayerList.All(p => p.IsMoved);
    }

    // 모든 몬스터가 이동했는지 체크
    private bool IsAllMonsterMoved()
    {
        return activeMonsterList.All(i => i.IsMoved);
    }


    private void GrantExtraTurn(Creature creature)
    {
        if (creature.IsAlive)
            extraTurnPlayerQueue.Enqueue(creature);
        
    }

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

    // 모든 플레이어 행동(이동) 끝났을 때 실행되는 함수
    private void RaiseAllPlayerMoveFinishEvent()
    {
        GameEvent AllmoveFinishEvent = Managers.Resource.Load<GameEvent>("AllmoveFinish"); // SO 생성
        if (AllmoveFinishEvent != null)
        {
            AllmoveFinishEvent.Raise(); // Raise = Inovke, Turn Manager -> 턴 종료 UI
        }
        else
        {
            Debug.LogWarning("AllmoveFinish GameEvent not found!");
        }
    }

    // 플레이어 이동 끝나고 실행되는 함수
    public void OnCreatureMoveFinish(GameObject receivedObject)
    {
        Debug.Log($"Turn Manager's OnMoveFinish at {receivedObject.name}");

        if (IsAllPlayerMoved() == true)
        {
            RaiseAllPlayerMoveFinishEvent();
        }
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

