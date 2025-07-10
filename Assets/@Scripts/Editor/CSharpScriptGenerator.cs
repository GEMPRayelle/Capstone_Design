using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class CSharpScriptGenerator : MonoBehaviour
{
    // % (Ctrl), # (Shift), & (Alt)
    [MenuItem("Tools/Create Scripts %#x")]
    private static void CreateScript()
    {
        string path = GetSelectedPathOrFallback();

        // 고유한 스크립트 경로 생성
        string finalPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "NewScript.cs"));

        // 파일 내용 작성
        File.WriteAllText(finalPath, GetTemplate("NewScript"));

        // 에셋 갱신 및 선택 포커스
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(finalPath);
    }

    // 현재 Project 창에서 선택한 경로를 가져옴. 없으면 null 반환
    private static string GetSelectedPathOrFallback()
    {
        string path = null;
        Object obj = Selection.activeObject;

        Debug.Log(obj);

        if (obj != null)
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                // 파일이면 상위 폴더 경로로 바꿈
                //path = Path.GetDirectoryName(path);
            }
        }

        return path;
    }

    private static string GetTemplate(string className) => $@"
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    public class {className} : MonoBehaviour
    {{
    
    }}";
}