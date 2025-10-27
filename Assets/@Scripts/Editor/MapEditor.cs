using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using static Define;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;


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

    [MenuItem("Tools/Create Object Tile Asset %#o")] //Ctrl Shift O
    public static void CreateObjectOnTile()
    {
        #region Monster
        //MonsterData 딕셔너리를 탐색
        Dictionary<int, Data.MonsterData> MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        foreach(var data in MonsterDic.Values)
        {
            if (data.DataId < 202000)
                continue;
            
            //Asset을 만들어서 Scriptable Object에 정보를 기입함
            //생성한 에셋이 Tile이 되어 팔레트에다가 올려놓고 맵툴로서 오브젝트를 찍어놓는 역할을 함
            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>(); //CustomTile 타입으로 에셋을 만들어줌
            customTile.Name = data.DescriptionTextID;
            string spriteName = data.IconImage;
            spriteName = spriteName.Replace(".sprite", "");

            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/@Resources/Sprites/Monsters/{spriteName}.png");
            customTile.sprite = spr;
            customTile.DataId = data.DataId;
            customTile.ObjectType = EObjectType.Monster;
            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/01_asset/Dev/Monster"; //새로 만들 에셋 경로
            path = Path.Combine(path, $"{name}.Asset");

            if (path == "")
                continue;

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }
        #endregion

        #region Player
        Dictionary<int, Data.HeroData> HeroDic = LoadJson<Data.HeroDataLoader, int, Data.HeroData>("HeroData").MakeDict();
        foreach (var data in HeroDic.Values)
        {
            if (data.DataId < 201000)
                continue;

            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            string spriteName = data.IconImage;
            spriteName = spriteName.Replace(".sprite", "");

            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/@Resources/Sprites/Players/{spriteName}/{spriteName}.png");
            customTile.sprite = spr;
            customTile.DataId = data.DataId;
            customTile.ObjectType = EObjectType.Player;
            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/01_asset/Dev/Players"; //새로 만들 에셋 경로
            path = Path.Combine(path, $"{name}.Asset");

            if (path == "")
                continue;

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }
        #endregion

        #region NPC
        Dictionary<int, Data.NpcData> Npc = LoadJson<Data.NpcDataLoader, int, Data.NpcData>("NpcData").MakeDict();
        foreach(var data in Npc.Values)
        {
            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            string spriteName = data.SpriteName;
            spriteName = spriteName.Replace(".sprite", "");

            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/@Resources/Sprites/NPC/{spriteName}.png");
            customTile.sprite = spr;
            customTile.Name = data.Name;
            customTile.DataId = data.DataId;
            customTile.ObjectType = EObjectType.Npc;
            string name = $"{data.DataId}_{data.Name}";
            string path = "Assets/@Resources/TileMaps/01_asset/dev/Npc"; //새로 만든 에셋을 넣을 경로
            path = Path.Combine(path, $"{name}.Asset");

            if (path == "")
                continue;

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }
        #endregion
    }

    //Json파일을 로드
    private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/@Resources/Data/JsonData/{path}.json");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
#endif
}
