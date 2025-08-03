using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//발사체 모션의 기본 베이스 클래스
public abstract class ProjectileMotionBase : InitBase
{
    Coroutine _coLaunchProjectile;

    #region 모션에서 알아야 할 정보들
    public Vector3 StartPosition { get; private set; }//시작 위치
    public Vector3 TargetPosition { get; private set; }//타겟 위치
    public bool isLookAtTarget { get; private set; }//sprite가 주시대상을 향하여 바라봐야 하는지
    public Data.ProjectileData ProjectileData { get; private set; }//추가적으로 가져와야 할 데이터들
    protected Action EndCallback { get; private set; }//어디까지 이동했을때 처리해줄 callback

    protected float _speed;
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void SetInfo(int projectileTemplateID, Vector3 spawnPosition, Vector3 targetPosition, Action endCallback = null)
    {
        _speed = 5.0f;

        if (projectileTemplateID != 0)
        {
            ProjectileData = Managers.Data.ProjectileDic[projectileTemplateID];
            _speed = ProjectileData.ProjSpeed;
        }
        StartPosition = spawnPosition;
        TargetPosition = targetPosition;
        EndCallback = endCallback;

        isLookAtTarget = true;//temp

        if (_coLaunchProjectile != null)
            StopCoroutine(_coLaunchProjectile);

        _coLaunchProjectile = StartCoroutine(CoLaunchProjectile());

    }

    //주시할 대상이 있다면 회전각을 만들어 방향을 바라보게 만들게한다
    protected void LookAt2D(Vector2 forward)
    {
        transform.rotation = Quaternion.Euler(0, 0, MathF.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }

    protected abstract IEnumerator CoLaunchProjectile();
}
