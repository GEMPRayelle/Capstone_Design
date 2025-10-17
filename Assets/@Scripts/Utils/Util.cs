using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using static Define;

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

    //4방향(상화좌우) 벡터를 리턴하는 함수 (4방향 벡터의 enum을 리턴함)
    public static IEnumerable<Vector2Int> GetDirection()
    {
        yield return Vector2Int.up;    // (0, 1)
        yield return Vector2Int.down;  // (0, -1)
        yield return Vector2Int.right; // (1, 0)
        yield return Vector2Int.left;  // (-1, 0)
    }
    
    public static Vector3 GetLookAtRotation(Vector3 dir)
    {
        //Mathf.Atan2를 사용해 각도를 계산하고, 라디안에서 디그리로 변환 
        float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;

        //Z축을 기준으로 회전하는 Vec3값을 리턴
        return new Vector3(0, 0, angle);
    }

    public static float GetEffectRadius(EEffectSize size)
    {
        //몬스터마다 피격을 정해주도록 함 
        switch (size)
        {
            case EEffectSize.CircleSmall:
                return EFFECT_SMALL_RADIUS;
            case EEffectSize.CircleNormal:
                return EFFECT_NORMAL_RADIUS;
            case EEffectSize.CircleBig:
                return EFFECT_BIG_RADIUS;
            case EEffectSize.ConeSmall:
                return EFFECT_SMALL_RADIUS * 2f;
            case EEffectSize.ConeNormal:
                return EFFECT_NORMAL_RADIUS * 2f;
            case EEffectSize.ConeBig:
                return EFFECT_BIG_RADIUS * 2f;
            default:
                return EFFECT_SMALL_RADIUS;
        }
    }
}
