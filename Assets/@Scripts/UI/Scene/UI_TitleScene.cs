using UnityEngine;
using static Define;

public class UI_TitleScene : UI_Scene
{

    enum GameObjects
    {
        //오브젝트 이름과 동일해야함
        StartImage
    }

    enum Texts
    {
        //오브젝트 이름과 동일해야함
        DisplayText
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        //Object들과 Text를 UI에 Binding
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        //StartImage를 클릭했을때 BindEvent에 넘길 함수
        GetObject((int)GameObjects.StartImage).BindEvent((evt) =>
        {
            Debug.Log("Change Scene");
            Managers.Scene.LoadScene(EScene.DevScene);
        });

        //로딩이 되기전엔 비활성화 처리
        GetObject((int)GameObjects.StartImage).gameObject.SetActive(false);
        GetText((int)Texts.DisplayText).text = $"";

        StartLoadAssets();

        return true;

    }

    void StartLoadAssets()
    {
        //원하는 타입의 리소스를 로드 가능하지만
        //어차피 처음은 전부 로드를 하기때문에 모든 에셋들의 최상위 클래스인 Object를 넣어줌
        //비동기로 로딩이 완료되면 그 다음 넣어주는 콜백 함수를 통해 이 함수가 실행되므로 인해 완료 통보를 받는것
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                //로딩이 끝나면 데이터 초기화
                Managers.Data.Init();

                //로딩이 끝나서 넘어갈 수 있게 되면 정상적으로 이미지랑 텍스트를 정상화처리
                GetObject((int)GameObjects.StartImage).gameObject.SetActive(true);
                GetText((int)Texts.DisplayText).text = $"Touch To Start";

                //로딩이 끝나면 아래처럼 데이터를 사용가능함
                //Managers.Data.TestDic;
            }
        });
    }
}
