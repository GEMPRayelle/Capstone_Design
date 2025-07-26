using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CameraController : InitBase
{
    private Player _servant;
    public Player Servant
    {
        get { return _servant; }
        set { _servant = value; }
    }

    private Player _master;
    public Player Master
    {
        get { return _master; }
        set { _master = value; }
    }

    private BaseObject _target;
    public BaseObject Target
    {
        get { return _target; }
        set { _target = value; }
    }

    [Header("Camera Settings")]
    public float transitionDuration = 1f; // 타겟 변경 시 전환 시간
    public Ease transitionEase = Ease.OutQuad; // 속도 변화 패턴 정의, 찾아보고 필요한걸로 변경

    private Tween _transitionTween; // 현재 진행 중인 전환 트윈
    private bool _isTransitioning = false; // 전환 중인지 여부

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 카메라 설정
        Camera.main.orthographicSize = 15.0f;
        Managers.Game.OnPlayerStateChanged -= HandleOnPlayerStateChanged;
        Managers.Game.OnPlayerStateChanged += HandleOnPlayerStateChanged;

        return true;
    }

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 newTargetPosition = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);

        if (!_isTransitioning)
        {
            // 전환 중이 아닐 때는 타겟과 정확히 같은 위치로 이동
            transform.position = newTargetPosition;
        }
        // 전환 중일 때는 DOTween이 카메라 위치를 제어하므로 여기서는 아무것도 하지 않음
    }

    private void HandleOnPlayerStateChanged(EPlayerState playerState)
    {
        switch (playerState)
        {
            case EPlayerState.None:
                break;
            case EPlayerState.Master:
                TargetChange(EPlayerState.Master);
                break;
            case EPlayerState.Servant:
                TargetChange(EPlayerState.Servant);
                break;
            default:
                break;
        }
    }

    private void TargetChange(EPlayerState playerState)
    {
        if (_servant == null || _master == null)
            return;

        BaseObject newTarget = null;

        if (playerState == EPlayerState.Servant)
            newTarget = _servant;
        else if (playerState == EPlayerState.Master)
            newTarget = _master;

        if (newTarget == null || newTarget == Target)
            return;

        // 이전 트윈이 있다면 정리
        if (_transitionTween != null && _transitionTween.IsActive())
        {
            _transitionTween.Kill();
        }

        Target = newTarget;
        Vector3 startPos = transform.position;

        _isTransitioning = true; // 이동 시작

        // DOTween을 사용해서 부드럽게 카메라 이동
        _transitionTween = DOTween.To(() => 0f, x => { // () => 0f -> 시작값 : 0, x => {} 매 프레임마다 실행되는 콜백(x는 0에서 1로 변화), 1f = 최종값
            if (Target != null)
            {
                Vector3 currentTargetPos = new Vector3(Target.CenterPosition.x, Target.CenterPosition.y, -10f);
                transform.position = Vector3.Lerp(startPos, currentTargetPos, x);
            }
        }, 1f, transitionDuration) // 1f = x의 최종값, transitionDuration = 애니메이션 지속 시간
        .SetEase(transitionEase) // 변환 타입 설정, 위에 정의에 주석 볼것
        .OnComplete(() => { // 끝나면
            _isTransitioning = false; // Transiting 끝
        });
    }

}