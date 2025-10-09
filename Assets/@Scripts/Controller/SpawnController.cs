using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Define;

public class SpawnController : InitBase
{
    private List<int> spawnablePlayerID; // order가 스폰할 캐릭터들 ID
    private List<OverlayTile> spwnableTiles;
    private OverlayTile clickedTile;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        spawnablePlayerID = new List<int>();

        spawnablePlayerID.Add(HERO_WIZARD_ID);
        spawnablePlayerID.Add(HERO_LION_ID);

        return true;
    }

    public bool isAllSpawned()
    {
        return (spawnablePlayerID.Count == 0);
    }

    public void InstantiatePlayerByOrder(GameObject tile) // 알람 오면 실행될 플레이어 생성 함수
    {
        OverlayTile overlayTile = tile.GetComponent<OverlayTile>();
        Player player = Managers.Object.Spawn<Player>(overlayTile.transform.position, spawnablePlayerID.First());
        player.currentStandingTile = overlayTile;
        player.PlayerType = EPlayerType.Offensive;
        player.currentStandingTile.isBlocked = true;
        player.currentStandingTile.HideTile();
        Managers.Turn.activePlayerList.Add(player);
        spawnablePlayerID.RemoveAt(0);

        if (isAllSpawned())
        {
            RiaseAllPlayerSpawnedEvent();
            // 이벤트 MouseController에 날려서 MouseController에서 이제 클릭 처리는 플레이어 변경, 이동처리되게 만들기
        }
    }

    public void RiaseAllPlayerSpawnedEvent()
    {
        GameEvent spawnFinishEvent = Managers.Resource.Load<GameEvent>("AllPlayerSpawned");

        if (spawnFinishEvent != null)
        {
            spawnFinishEvent.Raise();
        }

        else
        {
            Debug.Log("AllPlayerSpawned is NULL");
        }
    }
}

