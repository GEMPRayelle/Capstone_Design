using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

public class MapEditor : MonoBehaviour
{
    //타일에 대한 맵 정보 추출
#if UNITY_EDITOR
    [MenuItem("Tools/MapGenerate %#m")] //Ctrl Shift m
    private static void GenerateMap() //Collision 정보를 추출하는 용도
    {
        //씬에서(Hierarchy) 선택한 오브젝트만 포함
        GameObject[] gameObjects = Selection.gameObjects;
        foreach(GameObject go in gameObjects)
        {
            //go를 재귀적으로 순회해서 Tilemap_Collision이름의 타일맵을 찾음
            Tilemap tile = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            //MapData폴더에 txt파일을 생성해줌 (클라랑 서버랑 같이 사용할 데이터)
            using (var writer = File.CreateText($"Assets/@Resources/Data/MapData/{go.name}Collision.txt"))
            {
                //x와 y의 각 최대 최소 좌표
                writer.WriteLine(tile.cellBounds.xMin);
                writer.WriteLine(tile.cellBounds.xMax);
                writer.WriteLine(tile.cellBounds.yMin);
                writer.WriteLine(tile.cellBounds.yMax);

                //x,y좌표를 전부 순회하면서
                for (int y = tile.cellBounds.yMax; y >= tile.cellBounds.yMin; y--)
                {
                    for (int x = tile.cellBounds.xMin; x <= tile.cellBounds.xMax; x++)
                    {
                        //타일 정보를 가져옴
                        TileBase tileBase = tile.GetTile(new Vector3Int(x, y, 0));
                        //타일이 있으면
                        if (tileBase != null)
                        {
                            //타일 이름이 O가 포함되어 있다면
                            //TimeMap/01_asset/dev/Collider/terrain_O,X 파일임
                            if (tileBase.name.Contains("O"))
                                //모두가 갈수있는 영역
                                writer.Write(Define.MAP_TOOL_NONE);
                            else
                                //카메라만 갈 수 있는 영역
                                writer.Write(Define.MAP_TOOL_SEMI_WALL);
                        }
                        //타일이 없으면
                        else
                            //갈 수 없는 영역으로 표시
                            writer.Write(Define.MAP_TOOL_WALL);
                    }
                    writer.WriteLine();
                }
            }
        }
        Debug.Log("Map Collision Generation Complete");
    }

    //Json파일을 로드
    private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/@Resources/Data/JsonData/{path}.json");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
#endif
}
