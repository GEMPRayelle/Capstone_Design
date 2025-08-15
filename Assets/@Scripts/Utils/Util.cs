using UnityEngine;
using System;
using UnityEngine.Rendering.Universal;

public static class Util
{
    /// <summary>
    /// GameObject에 T라는 컴포넌트를 찾고 없으면 T 컴포넌트를 추가시키는 함수
    /// </summary>
    /// <returns>찾아서 없으면 그 컴포넌트를 추가하고 그 컴포넌트를 반환</returns>
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();

        return component;
    }

    /// <summary>
    /// GameOject의 자식 오브젝트를 반환시키는 함수
    /// </summary>
    /// <param name="name"></param>
    /// <param name="recursive">재귀적으로 Child의 Child를 찾을지</param>
    /// <returns></returns>
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    //string 16진수 번호를 받아서 Color r,g,b를 리턴시키는 함수
    public static Color HexToColor(string color)
    {
        if (color.Contains("#") == false)
            color = $"{color}";
        ColorUtility.TryParseHtmlString(color, out Color parsedColor);

        return parsedColor;
    }


}
