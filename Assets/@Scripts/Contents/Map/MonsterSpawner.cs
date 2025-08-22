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
    private float SpawnTime = 0.5f;
    private float elapsed = 0.0f;
    private float _spawnPadding;

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

        _spawnPadding = Camera.main.orthographicSize / 2.0f;
        _spawnCoroutine = StartCoroutine(SpawnObjects());

        return true;
    }

    private IEnumerator SpawnObjects()
    {
        while (true)
        {
            while (elapsed < SpawnTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }


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
            //Fix -> Spawn함수의 두번째 매개변수로 templateId를 넘겨야하기 때문에 임시 수정
            Monster monster = Managers.Object.Spawn<Monster>(target.position + SpawnPostionPadding, MONSTER_GOBLIN_ARCHER_ID);

            if (monster != null)
                _spawnObjects.Add(monster);

            elapsed = 0.0f;
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
                break;
            case EPlayerState.Servant:
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
