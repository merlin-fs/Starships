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

public class TestSpawn : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;

    [SerializeField]
    public AssetReferenceT<UnitConfig> Config;

    [SerializeField]
    Button m_BtnReload;

    [SerializeField]
    private int m_Count = 1;

    private EntityManager m_EntityManager;

    private void Awake()
    {
        m_BtnReload.onClick.AddListener(
            () =>
            {
                CreateEntities(m_Count);
            });
    }

    private async void StartBatle()
    {
        var prefab = m_EntityManager.World.GetOrCreateSystemManaged<PrefabSystem>();
        await prefab.IsDone();

        var config = !Config.IsValid()
            ? await Config.LoadAssetAsync().Task
            : (UnitConfig)Config.Asset;

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new SpawnTag()
        {
            Entity = config.Prefab,
            WorldTransform = WorldTransform.FromPosition(-10, 0, 0)
        });

        entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new SpawnTag()
        {
            Entity = config.Prefab,
            WorldTransform = WorldTransform.FromPosition(10, 0, 0)
        });
    }


    private async void CreateEntities(int count)
    {
        var config = !Config.IsValid()
            ? await Config.LoadAssetAsync().Task
            : (UnitConfig)Config.Asset;

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
