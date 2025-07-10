using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrensScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

    //Scene을 로드하는 함수
    public void LoadScene(Define.EScene type)
    {
        SceneManager.LoadScene(GetSceneName(type));
    }

    //SceneType을 string으로 변환하여 리턴
    private string GetSceneName(Define.EScene type)
    {
        string name = System.Enum.GetName(typeof(Define.EScene), type);
        return name;
    }

    public void Clear()
    {
        //CurrensScene.Clear();
    }
}
