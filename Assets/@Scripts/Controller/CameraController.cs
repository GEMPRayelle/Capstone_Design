using DG.Tweening;
using UnityEngine;

public class CameraController : InitBase
{
    private BaseObject _target;
    public BaseObject Target
    {
        get { return _target; }
        set
        {
            _target = value;
        }
    }

    private bool _isMoving = false; // DOTween 이동 중인지 체크

    private float _moveDuration = 0.5f; // 이동 시간
    private Ease _moveEase = Ease.OutQuad; // 이동 easing

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

        if (_isMoving) return; // DOTween 이동 중이면 LateUpdate 이동 스킵

        //그게 아니라면 target을 따라감
        Vector3 newTargetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);
        transform.position = newTargetPosition;
    }

    public void SetCameraTarget(GameObject creature)
    {
        if (creature)
        {
            Target = creature.GetComponent<Creature>();
            MoveToDotween();
        }
    }

    private void MoveToDotween()
    {
        if (Target == null) return;

        _isMoving = true;

        // 목표 위치 계산
        Vector3 targetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);

        // DOTween으로 부드럽게 이동
        transform.DOMove(targetPosition, _moveDuration)
            .SetEase(_moveEase)
            .OnComplete(() =>
            {
                _isMoving = false; // 이동 완료
            });
    }
}