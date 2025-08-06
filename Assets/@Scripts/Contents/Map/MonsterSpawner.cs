using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static Define;
public class MonsterSpawner : BaseObject
{
      
    [SerializeField]
    private List<BaseObject> _spawnObjects = new List<BaseObject>();

    public Rigidbody2D target;
    // 임시 코드
    private float SpawnTime = 5.0f;
    private float _spawnPadding;
    private EPlayerState activePlayerState = EPlayerState.None;

    private Coroutine _spawnCoroutine;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        GameObject player = GameObject.Find("@Players");

        if (player != null)
            target = player.transform.GetChild(0).GetComponent<Rigidbody2D>();


        Managers.Game.OnPlayerStateChanged -= HandleOnPlayerStateChanged;
        Managers.Game.OnPlayerStateChanged += HandleOnPlayerStateChanged;

        _spawnPadding = Camera.main.orthographicSize;
        _spawnCoroutine = StartCoroutine(SpawnObjects());

        return true;
    }

    // TODO 나중에 코루틴 멈추고 다시 멈춘 부분에서 실행되게 만들기
    private IEnumerator SpawnObjects()
    {
        // 임시 코드
        while (true)
        {
            int randomValue = Random.Range(0, 4);
            float SpawnPadding = _spawnPadding + 2.0f;
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
            //Fix -> Spawn함수의 두번째 매개변수로 templateId를 넘겨야하기 때문에 임시 수정
            Monster monster = Managers.Object.Spawn<Monster>(target.position + SpawnPostionPadding, MONSTER_GOBLIN_ARCHER_ID);
            
            if (monster != null)
                _spawnObjects.Add(monster);

            yield return new WaitForSeconds(SpawnTime);
        }
    }

    private void HandleOnPlayerStateChanged(EPlayerState playerstate)
    {
        StopSpawn();

        switch (playerstate)
        {
            case EPlayerState.None:
                break;
            case EPlayerState.Master:
                activePlayerState = EPlayerState.Master;
                break;
            case EPlayerState.Servant:
                activePlayerState = EPlayerState.Servant;
                StartSpawn();
                break;
        }
    }

    private void StopSpawn()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private void StartSpawn()
    {
        if (_spawnCoroutine == null)
        {
            _spawnCoroutine = StartCoroutine(SpawnObjects());
        }
    }

}
