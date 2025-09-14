using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CameraController : InitBase
{
    private BaseObject _target;
    public BaseObject Target
    {
        get { return _target; }
        set { _target = value; }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 카메라 설정
        Camera.main.orthographicSize = 15.0f;

        return true;
    }

    private void LateUpdate()
    {
        if (Target == null) return; //주시하는 타겟이 없으면 아무것도 안함

        //그게 아니라면 target을 따라감
        Vector3 newTargetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);
        transform.position = newTargetPosition;
    }
}