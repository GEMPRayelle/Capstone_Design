using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class Extension
{
    /// <summary>
    /// GameObject에 T라는 컴포넌트를 찾고 없으면 T 컴포넌트를 추가시키는 함수
    /// </summary>
    /// <returns>찾아서 없으면 그 컴포넌트를 추가하고 그 컴포넌트를 반환</returns>
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    /// <summary>
    /// UI의 이벤트를 받아 함수를 실행시켜줌
    /// </summary>
    /// <param name="go"></param>
    /// <param name="action">실행시킬 함수</param>
    /// <param name="type">무슨 타입의 이벤트인지</param>
    public static void BindEvent(this GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        UI_Base.BindEvent(go, action, type);
    }

    /// <summary>
    /// Transform의 확장 함수 (이동)
    /// </summary>
    public static void TranslateEx(this Transform transform, Vector3 dir)
    {
        BaseObject bo = transform.gameObject.GetComponent<BaseObject>();
        if (bo != null)
            bo.TranslateEx(dir);
    }

    /// <summary>
    /// GameObject가 무적인 상태인지 반환하는 함수
    /// </summary>
    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }

    /// <summary>
    /// BaseObject가 무적인 상태인지 반환하는 함수
    /// </summary>
    public static bool IsValid(this BaseObject bo)
    {
        if (bo == null || bo.isActiveAndEnabled == false)
            return false;

        //TODO : Creature 객체를 생성하고 BaseObject를 다운캐스팅 한 다음
        //creatureState가 dead상태가 아닌지 리턴하는 코드 추가

        return true;
    }

    /// <summary>
    /// 리스트를 받아 셔플시키는 함수
    /// </summary>
    /// <param name="list">셔플시킬 list</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]); //swap
        }
    }

    public static void MakeMask(this ref LayerMask mask, List<Define.ELayer> list)
    {
        foreach (Define.ELayer layer in list)
            mask |= (1 << (int)layer);
    }

    public static void AddLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask |= (1 << (int)layer);
    }

    public static void RemoveLayer(this ref LayerMask mask, Define.ELayer layer)
    {
        mask &= ~(1 << (int)layer);
    }
}
