using DG.Tweening;
using TMPro;
using UnityEngine;
using static Define;

public class DamageFont : MonoBehaviour
{
    private TextMeshPro _damageText; //데미지 텍스트

    public void SetInfo(Vector2 pos, float damage = 0, Transform parent = null, bool isCritical = false)
    {
        _damageText = GetComponent<TextMeshPro>();
        _damageText.sortingOrder = SortingLayers.PROJECTILE;

        transform.position = pos;

        //데미지가 음수면 힐
        if(damage < 0)
        {
            //초록색
            _damageText.color = Util.HexToColor("4EEE6F");
        }
        else if (isCritical)
        {
            //노란색
            _damageText.color = Util.HexToColor("EFAD00");
        }
        else
        {
            _damageText.color = Color.red;
        }

        _damageText.text = $"{Mathf.Abs(damage)}";
        _damageText.alpha = 1;

        DoAnimation();   
    }

    private void DoAnimation()
    {
        Sequence seq = DOTween.Sequence(); //여러개를 연달아 실행함

        transform.localScale = new Vector3(0, 0, 0);

        //첫번째 시퀀스에는 크기를 늘리고 폰트가 위로 올라가도록함
        seq.Append(transform.DOScale(1.3f, 0.3f).SetEase(Ease.InOutBounce)).
            Join(transform.DOMove(transform.position + Vector3.up, 0.3f).SetEase(Ease.Linear))

            //두번째 시퀀스는 크기를 줄이고 천천히 사라지게함
            .Append(transform.DOScale(1.0f, 0.3f).SetEase(Ease.InOutBounce)).
            Join(transform.GetComponent<TMP_Text>().DOFade(0, 0.3f).SetEase(Ease.InQuint))
            .OnComplete(() =>
            {
                //완료되면 폰트 프리팹 제거
                Managers.Resource.Destroy(gameObject);
            });
    }
}
