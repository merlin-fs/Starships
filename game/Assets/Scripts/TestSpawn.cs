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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Common.Core;

using UnityEngine.AddressableAssets;
using Game.Core.Spawns;

public class TestSpawn : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;

    [SerializeField]
    public AssetReferenceT<UnitConfig> Player;
    [SerializeField]
    public AssetReferenceT<UnitConfig> Enemy;

    private EntityManager m_EntityManager;
    
    private void StartBatle()
    {
        //m_EntityManager.WorldUnmanaged.ResolveSystemStateRef()
        //var system = SystemAPI.
        //var ecb = system.CreateCommandBuffer(state.WorldUnmanaged);
        //var prefab = m_EntityManager.World.GetOrCreateSystemManaged<PrefabInfo.System>();
        //!!! await prefab.IsDone();

        //*
        //var manager = new SaveManager((SavedContext)"Test");
        //manager.Load();
        /**/

        /*
        var player = !Player.IsValid()
            ? await Player.LoadAssetAsync().Task
            : (UnitConfig)Player.Asset;
        var enemy  = !Enemy.IsValid()
            ? await Enemy.LoadAssetAsync().Task
            : (UnitConfig)Enemy.Asset;

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var entity = ecb.CreateEntity();
        var transform = LocalTransform.FromPosition(0f, -3f, -1.54f);
        //transform.Rotation = quaternion.RotateY(-180);
        
        transform.Rotation = quaternion.RotateX(math.radians(-90));
        //transform.Rotation = math.mul(transform.Rotation, quaternion.RotateX(math.radians(-90)));

        if (player.Prefab == Entity.Null)
            return;

        ecb.AddBuffer<SpawnComponent>(entity);
        ecb.AppendToBuffer<SpawnComponent>(entity, ComponentType.ReadOnly<SavedTag>());
        ecb.AddComponent(entity, new NewSpawnWorld()
        {
            Prefab = player.Prefab,
            WorldTransform = transform,
        });

        var view = GetComponent<TestHealthView>();
        var viewObject = GameObject.Instantiate(view.View, view.Canvas.transform);
        var buff = ecb.AddBuffer<StatView>(entity);
        buff.Add(new StatView(viewObject, Stat.GetID(Global.Stat.Health)));
        /**/

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


    /*
    private async void CreateEntities(int count)
    {
        var config = !Enemy.IsValid()
            ? await Enemy.LoadAssetAsync().Task
            : (UnitConfig)Enemy.Asset;

        Debug.Log($"try spawn config: {config.EntityPrefab}");
        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        for (var i = 0; i < count; i++)
        {
            var entity = ecb.CreateEntity();
            ecb.AddBuffer<Spawn.Component>(entity);
            ecb.AddComponent(entity, new Spawn() { Prefab = config.EntityPrefab });
            ecb.AppendToBuffer<Spawn.Component>(entity, ComponentType.ReadOnly<SavedTag>());
        }
    }
    */

    private async void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 3;
        StartBatle();
    }


    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
