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
    Button m_BtnReload;

    private int m_Current;

    private EntityManager m_EntityManager;

    Vector3 RandomBetweenRadius2D(float minRad, float maxRad)
    {
        //RandomNormal* Mathf.Sqrt(Random.Range(0.0f, 1.0f))
        float radius = UnityEngine.Random.Range(minRad, maxRad);
        //float angle = UnityEngine.Random.Range(0, 360);
        float angle = UnityEngine.Random.Range(-90, 90);

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
                m_Current = m_Count;
                StartCoroutine(Spawn());
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

        if (enemy.Prefab == Entity.Null)
            return;

        var point = RandomBetweenRadius2D(10, 25 / 2) + new Vector3(0, 8, 0);
        //var point = RandomBetweenRadius2D(0, 1f) + new Vector3(0, 8, 0);
        var transform = LocalTransform.FromPosition(point);
        var entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new NewSpawnWorld()
        {
            Prefab = enemy.Prefab,
            WorldTransform = transform
        });
    }

    IEnumerator Spawn()
    {
        while (m_Current > 0)
        {
            StartSpawn();
            //var time = UnityEngine.Random.Range(0.01f, 1f);
            //yield return new WaitForSeconds(time);
            yield return null;
            m_Current--;
        }
    }

    private void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
}
