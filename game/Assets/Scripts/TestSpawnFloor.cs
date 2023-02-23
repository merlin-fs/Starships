using UnityEngine.UI;
using TMPro;
using Unity.Scenes;
using Unity.Entities;
using UnityEngine;
using Game.Core.Prefabs;
using Game;
using Game.Core.Repositories;
using System.Linq;
using Game.Model;
using Unity.Collections;
using System.Drawing;
using Unity.Mathematics;
using System;
using Common.Core;
using Common.Defs;
using Game.Model.Worlds;

public class TestSpawnFloor : MonoBehaviour
{
    private EntityManager m_EntityManager;

    private async void StartBatle()
    {
        var prefabs = m_EntityManager.World.GetOrCreateSystemManaged<PrefabEnvironmentSystem>();
        await prefabs.IsDone();

        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();

        var repo = Repositories.Instance.GetRepo("floor");
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
    }


    private void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        StartBatle();
    }
}
