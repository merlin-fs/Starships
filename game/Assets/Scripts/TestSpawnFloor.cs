using System;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine.UI;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

using Common.Core;
using Common.Defs;
using Game;
using Game.Core.Repositories;
using Game.Model.Worlds;
using Buildings;

using Reflex.Attributes;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
public class TestSpawnFloorStatic
{
    static TestSpawnFloorStatic()
    {
        Game.Core.EnumHandle.Manager.Initialize();
    }
}
#endif

public class TestSpawnFloor : MonoBehaviour
{
    [SerializeField] Button m_BtnSave;
    [SerializeField] Button m_BtnLoad;
    
    private EntityManager m_EntityManager;
    
    [Inject] private IApiEditor m_ApiEditor;
    [Inject] private ObjectRepository m_Repository;
    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Game.Core.EnumHandle.Manager.Initialize();
    }

    private void Start()
    {
        //m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //StartBatle();
        //AddNewFloor();
    }

    private async void StartBatle()
    {
        /*
        var list = await Task.WhenAll(RepositoryLoadSystem.LoadObjects(), RepositoryLoadSystem.LoadAnimations());
        await ReferenceSubSceneManager.LoadAsync();
        var ids = list.SelectMany(iter => iter).Select(iter => iter.ID);
        ReferenceSubSceneManager.LoadSubScenes(m_EntityManager.WorldUnmanaged, ids);
        
        await RepositoryLoadSystem.LoadObjects();
        await RepositoryLoadSystem.LoadAnimations();
        
        var prefabSystem = m_EntityManager.WorldUnmanaged.GetUnsafeSystemRef<PrefabInfo.System>(m_EntityManager.WorldUnmanaged.GetExistingUnmanagedSystem<PrefabInfo.System>());
        await prefabSystem.IsDone();

        var system = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();

        var def = new Map.Data.Def
        {
            Size = new int2(100, 100)
        };

        var entity = m_EntityManager.CreateSingleton<Map.Data>();
        var context = new EntityManagerContext(m_EntityManager);
        //DefHelper.AddComponentData<Map.Data>(ref def, entity, context);
        def.AddComponentData(entity, context);
        
        Map.Layers.AddLayer<Map.Layers.Door>(entity, context);
        Map.Layers.AddLayer<Map.Layers.Floor>(entity, context);
        Map.Layers.AddLayer<Map.Layers.Structure>(entity, context);
        Map.Layers.AddLayer<Map.Layers.UserObject, Map.Layers.UserObject.Validator>(entity, context);

        var map = m_EntityManager.GetAspect<Map.Aspect>(entity);
        map.Init(ref system.CheckedStateRef, map);
        */
        
        /*
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
        */
    }

}
