using UnityEditorInternal;
using UnityEngine;

//싱글톤의 T는 MonoBehaviour를 상속받은 클래스만 넣을 수 있음
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static bool Initialized { get; set; } = false; //초기화 여부
    private static T _instance; //싱글톤 인스턴스 

    public static T Instance { get { Init(); return _instance; } }

    //처음 초기화시 싱글톤을 상속한 Manager급 클래스를 게임 오브젝트로 생성
    public static void Init()
    {
        if (_instance == null && Initialized == false)
        {
            Initialized = true;

            GameObject go = GameObject.Find($"@{typeof(T).Name}");
            if (go == null)
            {
                go = new GameObject { name = $"@{typeof(T).Name}" };
                go.AddComponent<T>();
            }

            DontDestroyOnLoad(go);

            // 초기화
            _instance = go.GetComponent<T>();
        }
    }
}
