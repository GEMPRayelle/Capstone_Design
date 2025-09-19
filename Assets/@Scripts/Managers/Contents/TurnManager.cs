using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInfo 
{
    //Creature가 CanUseSkillList에 담긴 스킬을 사용할 수 있다는 정보와 현재 Grid 위치를 담고있음
    public Creature Creature {  get; private set; }
    public List<SkillBase> CanUseSkillList { get; private set; }
    public Vector2Int CellPos { get; private set; }

    public BattleInfo(Creature Player, List<SkillBase> canUseSkillList)
    {
        Creature = Player;
        CanUseSkillList = canUseSkillList;
    }
}

public class TurnManager 
{
    private int _currentTurn;
    public int CurrentTurn => _currentTurn;
    private List<BattleInfo> _battleInfoOrderList = new List<BattleInfo>();

    private void SetBattleInfoOrder()
    {
        //전투 정보 초기화
        _battleInfoOrderList.Clear();
        //전투 가능한 플레이어가 전부 다 살아있으면 정보 초기화
        foreach (Player player in Managers.Object.LivingPlayerList)
        {
            if (player.IsAlive)
            {
                _battleInfoOrderList.Add(new BattleInfo(player, player.GetUsableSkillList()));
            }
        }

        foreach (Monster monster in Managers.Object.LivingMonsterList)
        {
            _battleInfoOrderList.Add(new BattleInfo(monster, monster.GetUsableSkillList()));
        }
    }

    public IEnumerator CoTurnBattleHandler(Action onPlayerDead, Action onAllMonsterDead)
    {
        _currentTurn = 0;

        foreach (Player player in Managers.Object.LivingPlayerList)
        {
            if (player.IsAlive)
            {
            }
        }

        while (true)
        {
            //Player가 죽었는지
            if (Managers.Object.LivingPlayerList.Count == 0) 
            {
                onPlayerDead?.Invoke();
                yield break;
            }

            //Monster가 죽었는지
            if (Managers.Object.LivingMonsterList.Count == 0) 
            {
                onAllMonsterDead?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(0.5f);

            //그게 아니라면 전투 시작
            _currentTurn++;

            SetBattleInfoOrder();

            foreach(BattleInfo battleInfo in _battleInfoOrderList)
            {
                if (!battleInfo.Creature.IsAlive)
                {
                    continue;
                }

            }
        }
    }
}
