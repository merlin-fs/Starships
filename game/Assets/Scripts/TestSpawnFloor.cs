using System;
using UnityEngine.UI;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

using Common.Core;
using Common.Defs;
using Game;
using Game.Core.Prefabs;
using Game.Core.Repositories;
using Game.Model;
using Game.Model.Worlds;
using Buildings.Environments;

public class TestSpawnFloor : MonoBehaviour
{
    [SerializeField]
    Button m_Button;
    
    private EntityManager m_EntityManager;

    private void Start()
    {
        m_Button.onClick.AddListener(AddNewFloor);
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        StartBatle();
    }

    private async void AddNewFloor()
    {
        var prefabs = m_EntityManager.World.GetOrCreateSystemManaged<PrefabEnvironmentSystem>();
        await prefabs.IsDone();

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var repo = Repositories.Instance.GetRepo("Walls");
        var prefab = repo.FindByID(ObjectID.Create("Deck_Wall_snaps002"));
        var item = ecb.Instantiate(prefab.Prefab);
        ecb.AddComponent<SelectBuildingTag>(item);
    }

    private async void StartBatle()
    {
        var prefabs = m_EntityManager.World.GetOrCreateSystemManaged<PrefabEnvironmentSystem>();
        await prefabs.IsDone();

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var repo = Repositories.Instance.GetRepo("Floor");
        var prefab = repo.FindByID(ObjectID.Create("Deck_Floor_01_snaps002"));

        var def = new Map.Data.Def
        {
            Size = new int2(100, 100)
        };

        var entity = m_EntityManager.CreateSingleton<Map.Data>();
        def.AddComponentData(entity, new DefExt.EntityManagerContext(m_EntityManager));
        var map = m_EntityManager.GetAspect<Map.Aspect>(entity);
        map.Init();

        var arch = m_EntityManager.CreateArchetype(ComponentType.ReadWrite<SpawnMapTag>());
        int length = def.Size.x * def.Size.y;

        /*
        var entities = new NativeArray<Entity>(length, Allocator.Temp);
        m_EntityManager.CreateEntity(arch, entities);

        for (int i = 0; i < length; i++)
        {
            int2 pos;
            pos.y = i / def.Size.x;
            pos.x = i % def.Size.x;

            m_EntityManager.SetComponentData(entities[i], new SpawnMapTag
            {
                Prefab = prefab.Prefab,
                Position = pos,
            });
        }
        //ecb.Instantiate(prefab.Prefab);
        */
    }

}
