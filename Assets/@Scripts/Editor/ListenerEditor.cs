using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ListenerEditor : MonoBehaviour
{
    // Assets/@Resources/ScriptableObjects에 있는 SO들을 기반으로 Assets/@Resources/Prefabs/Listeners에 Listener을 자동으로 만들어주는 함수
    // SO를 활용해 사용법
    // 1. Assets/@Resources/ScriptableObjects에 SO 필요한거 만들기 ex) TestSO.asset
    // 2. 이 함수 컨트롤 + 쉬프트 + T 나 Tools에서 실행
    // 3. Assets/@Resources/Prefabs/Listeners에 ex) TestSO_Listener 파일이 만들어짊
    // 4. 해당 리스너를 Addressable에 등록
    // 5. 리스너 베이스가 된 SO(.asset)파일도 Addressable에 등록
    // 6. ListenerManager에 GetAllListenerNames함수의 string 배열에 추가한 리스너 이름 추가(Addressable에 등록된 이름과 같아야함)
    // 7. ListenerManager에 SetListenerResponse함수에서 타입별로 실행되는 Set~~ListenerResponse함수 들 중 내가 추가한 타입에 맞는 함수에 가서 
    // 7 계속. switch문에 내가 string 배열에 추가한 이름을 case로 둬서 이벤트 실행 시 알람 받아 실행될 함수 등록
    // 8. 지금까지 작업은 이벤트 Raise를 호출하면 실행될 함수는 등록된 것. 이제 이벤트 Raise를 호출할 곳에 필요한 SO만들어서 Raise 시키기. 
    // 8. ex) MouseController에서 조종하던 캐릭터 이동이 끝나면 MoveFinish SO를 Raize시키는 함수 RaiseMoveFinishEvent() 처럼 만들어야된다는것

    // 직접적인 제네릭 처리 불가 이유 : 유니티 SO는 직렬화 시스템을 사용하는데 이는 컴파일 타임에 타입이 확정 되어야함
    // 그런데 제네릭 처리는 런타임에 타입이 확정되서 직렬화 안됨. 그래서 사용불가
#if UNITY_EDITOR
    [MenuItem("Tools/Listener Generate %#t")] // Ctrl+Shift+T
    static void GenerateListenersForAllSO()
    {
        // ScriptableObjects 폴더 경로
        string soFolderPath = "Assets/@Resources/ScriptableObjects";
        if (Directory.Exists(soFolderPath) == false) // 경로를 못찾는다면
        {
            Debug.LogError($"ScriptableObjects folder not found: {soFolderPath}");
            return;
        }

        // 리스너 프리팹을 저장할 폴더 (필요시 수정)
        string listenerPrefabFolder = "Assets/@Resources/Prefabs/Listeners";
        if (Directory.Exists(listenerPrefabFolder) == false)
        {
            Directory.CreateDirectory(listenerPrefabFolder);
        }

        int createdCount = 0; // 저장할 리스너 프리팹 개수
        int totalCount = 0;

        // 모든 GameEvent 에셋 찾기
        // 인자 1(필터) : GameEvent type, 인자 2(경로) : 검색할 폴더 배열
        // 리턴은 고유값 guid 배열, 실제 경로가 아님
        string[] guids0 = AssetDatabase.FindAssets("t:GameEvent", new[] { soFolderPath });

        foreach (string guid in guids0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid); // 실제 경로
            GameEvent eventAsset = AssetDatabase.LoadAssetAtPath<GameEvent>(assetPath); // SO 로드

            if (eventAsset != null)
            {
                totalCount++;
                Debug.Log($"Found GameEvent: {eventAsset.name}"); // 찾은 SO 넣어주기

                string listenerName = $"{eventAsset.name}_Listener";
                string prefabPath = $"{listenerPrefabFolder}/{listenerName}.prefab";

                // 이미 프리팹이 존재하는지 확인
                if (File.Exists(prefabPath))
                {
                    Debug.Log($"Listener prefab already exists: {listenerName}");
                    continue;
                }

                // 리스너 GameObject 생성
                GameObject listenerGO = new GameObject(listenerName);
                GameEventListener listener = listenerGO.AddComponent<GameEventListener>();

                // 할당
                listener.Event = eventAsset;

                // 프리팹으로 저장
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(listenerGO, prefabPath);

                // 생성한 임시 오브젝트 삭제
                DestroyImmediate(listenerGO);

                createdCount++;
                Debug.Log($"Created GameEventListener: {listenerName}");
            }
        }

        // 모든 GameEventGameObject 에셋 찾기
        // 인자 1(필터) : GameEventGameObject type, 인자 2(경로) : 검색할 폴더 배열
        string[] guids1 = AssetDatabase.FindAssets("t:GameEventGameObject", new[] { soFolderPath });

        foreach (string guid in guids1)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid); // 실제 경로
            GameEventGameObject eventAsset = AssetDatabase.LoadAssetAtPath<GameEventGameObject>(assetPath); // SO 로드

            if (eventAsset != null)
            {
                totalCount++;
                Debug.Log($"Found GameEventGameObject: {eventAsset.name}"); // 찾은 SO 넣어주기

                string listenerName = $"{eventAsset.name}_Listener";
                string prefabPath = $"{listenerPrefabFolder}/{listenerName}.prefab";

                // 이미 프리팹이 존재하는지 확인
                if (File.Exists(prefabPath))
                {
                    Debug.Log($"Listener prefab already exists: {listenerName}");
                    continue;
                }

                // 리스너 GameObject 생성
                GameObject listenerGO = new GameObject(listenerName);
                GameEventGameObjectListener listener = listenerGO.AddComponent<GameEventGameObjectListener>();

                // 할당
                listener.Event = eventAsset;

                // 프리팹으로 저장
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(listenerGO, prefabPath);

                // 생성한 임시 오브젝트 삭제
                DestroyImmediate(listenerGO);

                createdCount++;
                Debug.Log($"Created GameEventGameObjectListener: {listenerName}");
            }
        }

        // GameEventGameObjectList도 함께 찾기
        string[] guids2 = AssetDatabase.FindAssets("t:GameEventGameObjectList", new[] { soFolderPath });

        foreach (string guid in guids2)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid); // 실제 경로
            GameEventGameObjectList eventAsset = AssetDatabase.LoadAssetAtPath<GameEventGameObjectList>(assetPath); // SO 로드

            if (eventAsset != null)
            {
                totalCount++;
                Debug.Log($"Found GameEventGameObjectList: {eventAsset.name}"); // 찾은 SO 넣어주기

                string listenerName = $"{eventAsset.name}_Listener";
                string prefabPath = $"{listenerPrefabFolder}/{listenerName}.prefab";

                // 이미 프리팹이 존재하는지 확인
                if (File.Exists(prefabPath))
                {
                    Debug.Log($"Listener prefab already exists: {listenerName}");
                    continue;
                }

                // 리스너 GameObject 생성
                GameObject listenerGO = new GameObject(listenerName);
                GameEventGameObjectListListener listener = listenerGO.AddComponent<GameEventGameObjectListListener>();

                // 할당
                listener.Event = eventAsset;

                // 프리팹으로 저장
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(listenerGO, prefabPath);

                // 생성한 임시 오브젝트 삭제
                DestroyImmediate(listenerGO);

                createdCount++;
                Debug.Log($"Created GameEventGameObjectListListener: {listenerName}");
            }
        }

        if (totalCount == 0)
        {
            Debug.LogWarning("No GameEvent assets found!");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Listener generation complete! Created {createdCount} new listeners out of {totalCount} total events.");
    }
#endif
}