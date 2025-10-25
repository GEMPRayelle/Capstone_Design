using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static ControllerManager;
using static Define;

public class PlayerMovementController : InitBase // 플레이어 이동 관리하는 컨트롤러
{
    GameEventGameObject moveFinishEvent; // 모든 캐릭터 이동 끝나면 Raise

    private SharedPlayerState PlayerState; // 공유 데이터
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        PlayerState = Managers.Controller.PlayerState;
        moveFinishEvent = Managers.Resource.Load<GameEventGameObject>("moveFinish");

        return true;
    }

    void LateUpdate()
    {
        HandleMovement(); // 이동 가능할 때 움직이는 함수
    }

    private void HandleMovement()
    {
        // 경로가 존재하고 이동 중이라면(이동 허락된 상태라면) 캐릭터 이동 처리
        if (PlayerState.path.Count > 0 && PlayerState.isMoving)
        {
            // 이동할땐 캐릭터 이동 범위 타일 숨기기
            //HideAllRangeTiles();
            //// 캐릭터 스킬 범위 타일도 숨기기
            //foreach (var tile in SkillRangeTiles)
            //{
            //    tile.HideTile();
            //}
            MoveAlongPath();
        }
        // 경로가 없거나, 이동중이 아닌경우
        else if ((PlayerState.path.Count == 0 || !PlayerState.isMoving) && PlayerState.creature != null)
        {
            //foreach (var tile in rangeFinderTiles) 왜 넣었었지?
            //{
            //    tile.ShowTile();
            //}
            PlayerState.isMoving = false;
        }
    }

    private void MoveAlongPath()
    {
        var current = PlayerState.creature.SkeletonAnim.AnimationState.GetCurrent(0);
        //Debug.Log($"Current Anim: {current.Animation.Name}, loop:{current.Loop}, trackTime: {current.TrackTime}");

        //프레임 기반으로 이동하도록 계산
        var step = PlayerState.creature.Speed * Time.deltaTime;

        float zIndex = PlayerState.path[0].transform.position.z;//렌더링 순서용 타일 z값 저장
        //캐릭터를 다음 타일 방향으로 이동
        PlayerState.creature.transform.position = Vector2.MoveTowards(PlayerState.creature.transform.position, PlayerState.path[0].transform.position, step);
        //z값 보정
        PlayerState.creature.transform.position = new Vector3(PlayerState.creature.transform.position.x, PlayerState.creature.transform.position.y, zIndex);

        //목표 타일에 거의 다 도착했을 경우
        if (Vector2.Distance(PlayerState.creature.transform.position, PlayerState.path[0].transform.position) < 0.00001f)
        {
            PositionCharacterOnLine(PlayerState.path[0]);//정확한 위치로 보정
            PlayerState.path.RemoveAt(0);//현재 타일 제거후 다음 타일 경로로 이동하게 함
        }

        // 경로가 끝났다면
        if (PlayerState.path.Count == 0)
        {
            PlayerState.isMoving = false; // 이동 종료
            PlayerState.creature.CreatureState = ECreatureState.Idle;
            PlayerState.creature.IsMoved = true;
            PlayerState.ResetRangeTiles();


            RaiseMoveFinishEvent(); // 캐릭터 하나 이동했을 때 보내는 event
        }
    }

    //캐릭터를 타일에 정확한 위치에 보내도록 보정시키는 함수
    private void PositionCharacterOnLine(OverlayTile tile)
    {
        //캐릭터 위치 설정 (약간 뒤로 띄움)
        PlayerState.creature.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z);
        //렌더링 순서 설정
        //_creature.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        //캐릭터가 서 있는 타일 저장

        // 전에 서있던 타일에 blocked 해체
        PlayerState.creature.currentStandingTile.isBlocked = false;
        // 새로 이동한 타일 설정
        PlayerState.creature.currentStandingTile = tile;
        // 새로 이동한 타일 blocked 설정
        PlayerState.creature.currentStandingTile.isBlocked = true;
    }

    // 이동 끝났을 때 실행되는 함수
    private void RaiseMoveFinishEvent()
    {
        if (moveFinishEvent != null)
        {
            moveFinishEvent.Raise(PlayerState.creature.gameObject); // Raise = Invoke 시키기
            // MouseController -> TurnManager
        }
        else
        {
            Debug.LogWarning("moveFinish GameEventGameObject not found!");
        }
    }
}

