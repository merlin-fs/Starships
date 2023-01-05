using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.Transforms;
using UnityEngine.UI;

using Game;
using Game.Model;
using Game.Model.Units;
using Game.Core.Prefabs;
using System.Collections;

public class TestSpawnMeteorite : MonoBehaviour
{
    [SerializeField]
    public AssetReferenceT<UnitConfig> Enemy;

    [SerializeField]
    private int m_Count = 1;
    [SerializeField]
    private float m_Time = 1f;

    [SerializeField]
    Button m_BtnReload;

    private int m_Current;

    private EntityManager m_EntityManager;

    Vector3 RandomBetweenRadius2D(float minRad, float maxRad)
    {
        //RandomNormal* Mathf.Sqrt(Random.Range(0.0f, 1.0f))
        float radius = UnityEngine.Random.Range(minRad, maxRad);
        float angle = UnityEngine.Random.Range(0, 360);

        float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
        float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

        return new Vector3(x, y, 0);
    }

    Vector3 RandomBetweenRadius3D(float minRad, float maxRad)
    {
        float diff = maxRad - minRad;
        Vector3 point = Vector3.zero;
        while (point == Vector3.zero)
        {
            point = UnityEngine.Random.insideUnitSphere;
        }
        point = point.normalized * (UnityEngine.Random.value * diff + minRad);
        return point;
    }

    private void Awake()
    {
        m_BtnReload.onClick.AddListener(
            () =>
            {
                StartSpawn();
            });
    }

    private async void StartSpawn()
    {
        var prefab = m_EntityManager.World.GetOrCreateSystemManaged<PrefabSystem>();
        await prefab.IsDone();

        var enemy  = !Enemy.IsValid()
            ? await Enemy.LoadAssetAsync().Task
            : (UnitConfig)Enemy.Asset;

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var point = RandomBetweenRadius2D(5, 25 / 2);
        var transform = WorldTransform.FromPosition(point);
        var entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new SpawnTag()
        {
            Entity = enemy.Prefab,
            ConfigID = enemy.ID,
            WorldTransform = transform
        });
    }

    IEnumerator Spawn()
    {
        while (m_Current > 0)
        {
            StartSpawn();
            var time = UnityEngine.Random.Range(0.5f, m_Count);
            yield return new WaitForSeconds(time);
            m_Current--;
        }
    }

    private void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        m_Current = m_Count;
        //StartCoroutine(Spawn());
    }
}
