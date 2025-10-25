using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static ControllerManager;
using static Define;

public class SpawnController : InitBase
{
    private Player _copy; // order 소환할 때 캐릭터의 preview 당담
    private SharedPlayerState PlayerState;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        PlayerState = Managers.Controller.PlayerState;

        return true;
    }

    #region MouseController->Update에서 직접 참조 실행 함수
    public void UpdateCopyPosition(OverlayTile tile)
    {
        _copy.transform.position = tile.transform.position;
        _copy.currentStandingTile = tile;
        _copy.gameObject.SetActive(true);
    }

    public void HideCopy()
    {
        if (_copy != null)
            _copy.gameObject.SetActive(false);
    }

    public void HandleSpawnPreviewCopy(OverlayTile tile) // 소환할때 마우스 tile에 hover상태일때 실행
    {
        // copy가 있다면 위치와 init할거 하기
        if (_copy != null)
        {
            UpdateCopyPosition(tile);
        }
    }
    
    public bool IsCopyValid()
    {
        if (_copy) 
            return true;
        else
            return false;
    }

    public OverlayTile GetCopyStandingTile()
    {
        if (_copy)
        {
            return _copy.currentStandingTile;
        }

        else
        {
            return null;
        }
    }
    #endregion

    #region GameEvent를 통해 호출
    public void DespawnCopy()
    {
        if (_copy != null)
            Managers.Object.Despawn<Player>(_copy);
    }

    public void SwitchToOrderForSpawn(GameObject go)
    {
        OverlayTile tile = go.GetComponent<OverlayTile>();
        DespawnCopy();
        _copy = InstantiateCopyPlayer(tile); // 실루엣 캐릭터 생성
    }


    /// 아마 GameEvent로 고쳐야함
    public void SwitchCopy(GameObject go)
    {
        OverlayTile tile = go.GetComponent<OverlayTile>();
        DespawnCopy();
        _copy = InstantiateCopyPlayer(tile, PlayerState.creature); // 실루엣 캐릭터 생성
    }

    // Order의 다른 Playable 캐릭터 실루엣 생성
    private Player InstantiateCopyPlayer(OverlayTile tile) // tile위치에 order가 스폰할 캐릭터 실루엣 생성
    {
        if (PlayerState.IsSpawnEnd()) return null;

        Player player = Managers.Object.Spawn<Player>(tile.transform.position, PlayerState.spawnablePlayerID.First());
        player.currentStandingTile = tile;
        // TODO 실루엣 처리
        player.GetComponent<CircleCollider2D>().enabled = false;
        Destroy(player.GetComponent<Rigidbody2D>());
        player.gameObject.SetActive(false);
        return player;
    }

    // Order의 다른 Playable 캐릭터 실루엣 생성
    private Player InstantiateCopyPlayer(OverlayTile tile, Player original) // tile위치에 order가 스폰할 캐릭터 실루엣 생성
    {
        Player player = Managers.Object.Spawn<Player>(tile.transform.position, original.DataTemplateID);
        player.currentStandingTile = tile;
        // TODO 실루엣 처리
        player.GetComponent<CircleCollider2D>().enabled = false;
        Destroy(player.GetComponent<Rigidbody2D>());
        player.gameObject.SetActive(false);
        return player;
    }

    // Order의 다른 Playable 캐릭터 생성
    public void InstantiatePlayerByOrder(GameObject go)
    {
        OverlayTile tile = go.GetComponent<OverlayTile>();
        Player player = Managers.Object.Spawn<Player>(tile.transform.position, PlayerState.spawnablePlayerID.First());
        player.currentStandingTile = tile;
        player.PlayerType = EPlayerType.Offensive; // TODO : 나중에 데이터 시트에 추가해서 SetInfo에서 설정되도록
        player.currentStandingTile.isBlocked = true;
        player.currentStandingTile.HideTile();
        Managers.Turn.activePlayerList.Add(player);
        PlayerState.spawnablePlayerID.RemoveAt(0);

        DespawnCopy();

        _copy = InstantiateCopyPlayer(tile); // 새로운 실루엣 캐릭터 생성
    }
    #endregion


}
