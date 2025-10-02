using UnityEngine;
using UnityEngine.Events;

public class GameEventListenerInternal
{
    public GameEvent gameEvent; //구독할 게임 이벤트 객체
    public UnityEvent response; //이벤트 발생시 콜백할 유니티이벤트 (인스펙터에서 조절)

    /// <summary>
    /// 이벤트 등록 함수
    /// </summary>
    /// <param name="gameEvent">구독할 GameEvent</param>
    /// <param name="response">이벤트 발생 시 실행할 UnityEvent</param>
    public void RegisterEvent(GameEvent gameEvent, UnityEvent response)
    {
        if (gameEvent != null)
        {
            this.gameEvent = gameEvent;
            this.response = response;

            // 이벤트에 자신을 리스너로 등록
            gameEvent.RegisterListener(this);
        }
    }

    /// <summary>
    /// 이벤트 등록 해제 함수
    /// </summary>
    public void UnregisterEvent()
    {
        if (gameEvent != null)
        {
            gameEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 이벤트 발생 시 호출되는 함수
    /// </summary>
    public void OnEventRaised()
    {
        if (response != null)
        {
            // 특정 이벤트 이름은 로그 제외
            if (gameEvent.name != "CorsorEvent" && gameEvent.name != "FocusedOnNewTile")
            {
                int persistentCount = response.GetPersistentEventCount(); // 이건 정적으로 추가한 함수 개수만 가져옴
                                                                          // 그래서 우리 코드는 런타임에 동적으로 함수를 등록해서 0으로 가져와짐

                if (persistentCount > 0)
                {
                    Debug.Log("Received: " + gameEvent.name + ". \r\n Calling: " +
                        response.GetPersistentTarget(0) + " -> " +
                        response.GetPersistentMethodName(0));
                }
                else
                {
                    Debug.Log("Received: " + gameEvent.name + " (runtime listener)");
                }
            }

            // UnityEvent 실행
            response.Invoke();
        }
    }
}

// 제네릭 타입을 지원하는 이벤트 리스너 클래스
public class GameEventListenerInternal<T> 
{
    // 구독할 제네릭 이벤트 객체
    private GameEvent<T> gameEvent;

    // 이벤트 발생 시 실행할 UnityEvent<T>
    private UnityEvent<T> response;

    /// <summary>
    /// 이벤트 등록 함수
    /// </summary>
    /// <param name="gameEvent">구독할 GameEvent<T></param>
    /// <param name="response">이벤트 발생 시 실행할 UnityEvent<T></param>
    public void RegisterEvent(GameEvent<T> gameEvent, UnityEvent<T> response)
    {
        if (gameEvent != null)
        {
            this.gameEvent = gameEvent;
            this.response = response;

            // 이벤트에 자신을 리스너로 등록
            gameEvent.RegisterListener(this);
        }
    }

    /// <summary>
    /// 이벤트 등록 해제 함수
    /// </summary>
    public void UnregisterEvent()
    {
        if (gameEvent != null)
        {
            gameEvent.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 이벤트 발생 시 호출되는 함수 (파라미터 포함)
    /// </summary>
    /// <param name="param">이벤트에서 전달된 파라미터</param>
    public void OnEventRaised(T param)
    {
        if (response != null)
        {
            // 특정 이벤트 이름은 로그 제외
            if (gameEvent.name != "FocusOnTile")
            {
                // Persistent listener 개수 확인
                int persistentCount = response.GetPersistentEventCount(); // 이건 정적으로 추가한 함수 개수만 가져옴
                                                                          // 그래서 우리 코드는 런타임에 동적으로 함수를 등록해서 0으로 가져와짐

                if (persistentCount > 0)
                {
                    Debug.Log("Received: " + gameEvent.name + ". \r\n Calling: " +
                        response.GetPersistentTarget(0) + " -> " +
                        response.GetPersistentMethodName(0));
                }
                else
                {
                    // 런타임 리스너는 정보를 출력할 수 없음
                    Debug.Log("Received: " + gameEvent.name + " (runtime listener)");
                }
            }

            // UnityEvent<T> 실행
            response.Invoke(param);
        }
    }
}
