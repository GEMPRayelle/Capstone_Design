using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static Define;
public class TestStage : BaseObject
{
      
    [SerializeField]
    private List<BaseObject> _spawnObjects = new List<BaseObject>();

    public Rigidbody2D target;
    Vector2 _moveDir = Vector2.zero;
    // 임시 코드
    private float SpawnTime = 1.0f;
    private float _spawnPadding;
    private float Speed = 5.0f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        return true;
    }
    private void Start()
    {
        GameObject player = GameObject.Find("Hero");

        if (player != null)
            target = player.GetComponent<Rigidbody2D>();

        _spawnPadding = Camera.main.orthographicSize;
        StartCoroutine(SpawnObjects());
    }

    // 나중에 플레이어 킬이나, 실행시간, 플레이어 레벨? 그런거에 따라 다른 몬스터 스폰되도록 변경
    private IEnumerator SpawnObjects()
    {
        // 임시 코드
        while (true)
        {
            int randomValue = Random.Range(0, 4);
            float SpawnPadding = _spawnPadding;
            Vector2 SpawnPostionPadding = Vector2.zero;
            switch (randomValue)
            {
                case 0:
                    SpawnPostionPadding = new Vector2(-SpawnPadding, SpawnPadding);
                    break;
                case 1:
                    SpawnPostionPadding = new Vector2(SpawnPadding, -SpawnPadding);
                    break;
                case 2:
                    SpawnPostionPadding = new Vector2(SpawnPadding, SpawnPadding);
                    break;
                case 3:
                    SpawnPostionPadding = new Vector2(-SpawnPadding, -SpawnPadding);
                    break;

            }
            Monster monster = Managers.Object.Spawn<Monster>(target.position + SpawnPostionPadding);
            
            if (monster != null)
                _spawnObjects.Add(monster);

            yield return new WaitForSeconds(SpawnTime);
        }
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;
    }

    public void Update()
    {
        transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
    }
}
