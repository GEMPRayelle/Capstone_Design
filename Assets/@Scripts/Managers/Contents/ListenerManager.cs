using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ListenerManager
{
    public UI_GameScene GameScene;
    
    public void InitGameScene()
    {
        GameScene = Util.FindChild<UI_GameScene>(Managers.UI.Root, "UI_GameScene");
    }

    public void InstantiateListener()
    {
        // Addressable에 등록된 모든 리소스 키 가져오기
        // 실제로는 "_Listener"로 끝나는 것들만 필요하므로
        // Addressable Label을 사용하는게 좋아보임

        // 방법 1: 미리 정의된 리스너 이름 리스트 사용
        string[] listenerNames = GetAllListenerNames(); // 이 함수는 아래에서 구현

        foreach (string listenerName in listenerNames)
        {
            // 리스너 생성
            GameObject go = Managers.Resource.Instantiate(listenerName, Managers.Object.ListenerRoot);

            if (go == null)
            {
                Debug.LogWarning($"Failed to instantiate listener: {listenerName}");
                continue;
            }

            // GameEventListener 타입 확인 및 추가
            GameEventListener basicListener = go.GetComponent<GameEventListener>();
            if (basicListener != null)
            {
                if (basicListener.Response == null)
                {
                    basicListener.Response = new UnityEvent();
                }
                Managers.Object.GameEventListeners.Add(basicListener);
                Debug.Log($"Added GameEventListener: {listenerName}");
                continue;
            }

            // GameEventGameObjectListener 타입 확인 및 추가
            GameEventGameObjectListener goListener = go.GetComponent<GameEventGameObjectListener>();
            if (goListener != null)
            {
                if (goListener.Response == null)
                {
                    goListener.Response = new UnityEvent<GameObject>();
                }

                Managers.Object.GameEventGameObjectListeners.Add(goListener);
                Debug.Log($"Added GameEventGameObjectListener: {listenerName}");
                continue;
            }

            // GameEventGameObjectListListener 타입 확인 및 추가, 젤 적게 쓰는거 밑에 두기(탐색 제일 오래 걸리니까)
            GameEventGameObjectListListener goListListener = go.GetComponent<GameEventGameObjectListListener>();
            if (goListListener != null)
            {
                if (goListListener.Response == null)
                {
                    goListListener.Response = new UnityEvent<List<GameObject>>();
                }

                Managers.Object.GameEventGameObjectListListeners.Add(goListListener);
                Debug.Log($"Added GameEventGameObjectListListener: {listenerName}");
                continue;
            }

            Debug.LogWarning($"Listener has no recognized component: {listenerName}");
        }

        SetListenerResponse();
    }

    // 각 리스너에 콜백에 대응으로 실행될 함수 등록 (처음 초기화 부분)
    public void SetListenerResponse()
    {
        string[] listenerNames = GetAllListenerNames();

        foreach (string listenerName in listenerNames)
        {
            // GameEventListener 타입 처리
            GameEventListener basicListener = Managers.Object.GameEventListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (basicListener != null)
            {
                SetBasicListenerResponse(basicListener, listenerName);
                continue;
            }

            // GameEventGameObjectListener 타입 처리
            GameEventGameObjectListener goListener = Managers.Object.GameEventGameObjectListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (goListener != null)
            {
                SetGameObjectListenerResponse(goListener, listenerName);
                continue;
            }

            // GameEventGameObjectListListener 타입 처리
            GameEventGameObjectListListener goListListener = Managers.Object.GameEventGameObjectListListeners
                .FirstOrDefault(l => l.gameObject.name == listenerName);
            if (goListListener != null)
            {
                SetGameObjectListListenerResponse(goListListener, listenerName);
                continue;
            }

            Debug.LogWarning($"Listener not found in any collection: {listenerName}");
        }
    }


    private void SetBasicListenerResponse(GameEventListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            case "AllmoveFinish_Listener": // 플레이어 캐릭터 모두가 이동(행동)했을 때
                // 필요하면 추가
                break;
            case "EndTurn_Listener": // 적군 endTurn
                listener.Response.AddListener(Managers.Turn.EndTurn); // Creature -> TurnManager
                break;
            case "EndPlayerTurn_Listener": // Player endTurn 버튼 눌릴때
                listener.Response.AddListener(Managers.Turn.OnPlayerTurnEnd); // EndTurnBtn -> TurnManager
                listener.Response.AddListener(Managers.Controller.mouseController.EndPlayerEvent); // EndTurnBtn -> MouseController
                break;
            case "StartPlayerTurn_Listener": // 플레이어 턴 시작할 때
                listener.Response.AddListener(GameScene.activeTurnEndBtn); // TurnManager -> EndTurnBtn(UI_GameScene에 있음)
                //listener.Response.AddListener()
                break;
            case "AllPlayerSpawn_Listener": // 모든 플레이어 소환됬을 때
                // 턴 매니저 턴 시작? 추가
                listener.Response.AddListener(GameScene.activeTurnEndBtn); // MouseController -> EndTurnBtn
                break;
            case "DespawnCopy_Listener":
                listener.Response.AddListener(Managers.Controller.spawnController.DespawnCopy);
                break;
            case "ShowRangeTiles_Listener":
                listener.Response.AddListener(Managers.Controller.tileEffectController.ShowRangeTiles);
                break;
            case "HighlightSpawnTile_Listener":
                listener.Response.AddListener(Managers.Controller.tileEffectController.HighlightSpawnTile);
                break;
            case "HideAllRangeTiles_Listener":
                listener.Response.AddListener(Managers.Controller.tileEffectController.HideAllRangeTiles);
                break;
            // 다른 기본 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for basic listener: {listenerName}");
                break;
        }
    }

    // GameEventGameObjectListener에 대한 Response 설정
    private void SetGameObjectListenerResponse(GameEventGameObjectListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent<GameObject>();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            case "moveFinish_Listener": // 한 캐릭터 이동(행동) 끝났을 때
                listener.Response.AddListener(Managers.Turn.OnCreatureMoveFinish); // MouseController -> TurnManager
                Debug.Log($"Set response for {listenerName}");
                break;
            case "ChangePlayer_Listener":
                //listener.Response.AddListener(Managers.Controller.playerMovementController.ChangePlayer);
                break;
            case "switchToOrderForSpawn_Listener":
                listener.Response.AddListener(Managers.Controller.spawnController.SwitchToOrderForSpawn);
                break;
            case "InstantiatePlayerByOrder_Listener":
                listener.Response.AddListener(Managers.Controller.spawnController.InstantiatePlayerByOrder);
                break;

            // 필요 시 추가
            //case "playerSpawn_Listener":
            //    listener.Response.AddListener(Managers.Object.SetPlayerListenerResponse);
            //    Debug.Log($"Set response for {listenerName}");
            //    break;

            // 다른 GameObject 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for GameObject listener: {listenerName}");
                break;
        }
    }

    // GameEventGameObjectListListener에 대한 Response 설정
    private void SetGameObjectListListenerResponse(GameEventGameObjectListListener listener, string listenerName)
    {
        if (listener.Response == null)
        {
            listener.Response = new UnityEvent<List<GameObject>>();
        }

        // 리스너 이름에 따라 적절한 콜백 등록
        switch (listenerName)
        {
            // 필요 시 추가
            case "MoveAlongPath_Listener":
                listener.Response.AddListener(Managers.Controller.movementController.MoveCharacterCommand); // Creature -> MovemetController
                break;
            // 다른 GameObject List 이벤트 리스너 추가
            default:
                Debug.LogWarning($"No response defined for GameObject List listener: {listenerName}");
                break;
        }
    }
    // 모든 리스너 이름을 반환하는 헬퍼 함수
    private string[] GetAllListenerNames()
    {
        // 일단 하드코딩
        return new string[]
        {
            // Basic_Event
            "AllmoveFinish_Listener",
            "EndTurn_Listener",
            "EndPlayerTurn_Listener",
            "StartPlayerTurn_Listener",
            "AllPlayerSpawn_Listener",
            "DespawnCopy_Listener",
            "ShowRangeTiles_Listener",
            "HighlightSpawnTile_Listener",
            "HideAllRangeTiles_Listener",

            // GameObject_Event
            "moveFinish_Listener",
            "ChangePlayer_Listener",
            "switchToOrderForSpawn_Listener",
            "InstantiatePlayerByOrder_Listener",
            // GameObjectList_Event
            "MoveAlongPath_Listener",
            // 필요한 리스너들 추가
        };

        // TODO : Addressable Label 사용
        // Addressable에서 "Listener" 라벨이 붙은 것들을 찾아서 반환
        // 이 경우 ResourceManager에 Label로 검색하는 함수가 필요
    }


}
