using UnityEngine.UI;
using TMPro;
using Unity.Scenes;
using Unity.Entities;
using UnityEngine;

using Game;
using Game.Model;
using Game.Model.Units;
using Game.Systems;
using Game.Core.Repositories;
using System;
using UnityEngine.AddressableAssets;
using Unity.Transforms;
using Game.Core.Prefabs;
using Unity.Mathematics;
using Game.Views.Stats;
using Game.Model.Stats;

public class TestSpawn : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;

    [SerializeField]
    public AssetReferenceT<UnitConfig> Player;
    [SerializeField]
    public AssetReferenceT<UnitConfig> Enemy;

    private EntityManager m_EntityManager;

    private async void StartBatle()
    {
        var prefab = m_EntityManager.World.GetOrCreateSystemManaged<PrefabSystem>();
        await prefab.IsDone();

        var player = !Player.IsValid()
            ? await Player.LoadAssetAsync().Task
            : (UnitConfig)Player.Asset;
        var enemy  = !Enemy.IsValid()
            ? await Enemy.LoadAssetAsync().Task
            : (UnitConfig)Enemy.Asset;

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var entity = ecb.CreateEntity();
        var transform = WorldTransform.FromPosition(0f, -3f, -1.54f);
        //transform.Rotation = quaternion.RotateY(-180);
        
        transform.Rotation = quaternion.RotateX(math.radians(-90));
        //transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(-90)));

        if (player.Prefab == Entity.Null)
            return;

        ecb.AddComponent(entity, new SpawnTag()
        {
            Entity = player.Prefab,
            WorldTransform = transform,
        });

        var view = GetComponent<TestHealthView>();
        var viewObject = GameObject.Instantiate(view.View, view.Canvas.transform);
        var buff = ecb.AddBuffer<StatView>(entity);
        buff.Add(new StatView(viewObject, Stat.GetID(GlobalStat.Health)));


        /*
        transform = WorldTransform.FromPosition(10, 0, 0);
        entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new SpawnTag()
        {
            Entity = enemy.Prefab,
            WorldTransform = transform
        });
        */
    }


    private async void CreateEntities(int count)
    {
        var config = !Enemy.IsValid()
            ? await Enemy.LoadAssetAsync().Task
            : (UnitConfig)Enemy.Asset;

        Debug.Log($"try spawn config: {config.Prefab}");
        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        for (var i = 0; i < count; i++)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SpawnTag() { Entity = config.Prefab });
        }
    }

    private async void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        /*
        var guid = SceneSystem.GetSceneGUID(ref m_EntityManager.WorldUnmanaged.GetExistingSystemState<SceneSystem>(), "Assets/Scenes/SampleScene/New Sub Scene.unity");
        //var loadParameters = new SceneSystem.LoadParameters { Flags = SceneLoadFlags.LoadAdditive | SceneLoadFlags.NewInstance };
        var sceneEntity = SceneSystem.LoadSceneAsync(m_EntityManager.WorldUnmanaged, guid);
        //SceneSystem.LoadPrefabAsync(m_EntityManager.WorldUnmanaged, guid);
        */
        await Repositories.Instance.ConfigsAsync();

        StartBatle();
    }


    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
