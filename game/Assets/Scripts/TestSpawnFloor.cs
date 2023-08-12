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
using Game.Core.Prefabs;
using Game.Core.Repositories;
using Game.Model;
using Game.Model.Worlds;
using Buildings;
using Game.Core.Saves;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
public class TestSpawnFloorStatic
{
    static TestSpawnFloorStatic()
    {
        Game.Core.Animations.Animation.Init();
        Game.Core.EnumHandle.Manager.Initialize();
    }
}
#endif

public class TestSpawnFloor : MonoBehaviour
{
    [SerializeField] Button m_BtnSave;
    [SerializeField] Button m_BtnLoad;
    
    private EntityManager m_EntityManager;
    
    DIContext.Var<IApiEditor> m_ApiEditor;
    DIContext.Var<ObjectRepository> m_Repository;
    DIContext.Var<ReferenceSubSceneManager> m_ReferenceSubSceneManager;
    
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Game.Core.Animations.Animation.Init();
        Game.Core.EnumHandle.Manager.Initialize();
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP_RUNTIME_WORLD
        DefaultWorldInitialization.Initialize("Default World", false);
#endif
    }

    private void Start()
    {
        m_BtnSave.onClick.AddListener(Save);
        m_BtnLoad.onClick.AddListener(Load);
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        StartBatle();
        //AddNewFloor();
    }

    private void Load()
    {
        var manager = new SaveManager((SavedContext)"Test");
        manager.Load();
    }    
    private void Save()
    {
        var manager = new SaveManager((SavedContext)"Test");
        manager.Save();
    }

    private readonly struct SavedContext: ISavedContext
    {
        private readonly string m_Name;
        public string Name => m_Name;
        public SavedContext(string name) => m_Name = name;

        public static implicit operator SavedContext(string name) => new SavedContext(name);
    }

    private async void AddNewFloor()
    {
        var system = m_EntityManager.WorldUnmanaged.GetUnsafeSystemRef<PrefabInfo.System>(m_EntityManager.WorldUnmanaged.GetExistingUnmanagedSystem<PrefabInfo.System>());
        //var system = m_EntityManager.WorldUnmanaged.GetUnsafeSystemRef<PrefabEnvironmentSystem>(m_EntityManager.WorldUnmanaged.GetExistingUnmanagedSystem<PrefabEnvironmentSystem>());
        await system.IsDone();
        var config = m_Repository.Value.FindByID("Deck_Wall_snaps002");
        m_ApiEditor.Value.AddEnvironment(config);
        /*
        var ecb = m_EntityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();
        var item = ecb.CreateEntity();
        ecb.AddComponent(item, new SpawnMapTag
        {
             Prefab = prefab.Prefab,
        });
        */
    }

    private async void StartBatle()
    {
        var list = await Task.WhenAll(RepositoryLoadSystem.LoadObjects(), RepositoryLoadSystem.LoadAnimations());
        await m_ReferenceSubSceneManager.Value.LoadAsync();
        var ids = list.SelectMany(iter => iter).Select(iter => iter.ID);
        m_ReferenceSubSceneManager.Value.LoadSubScenes(m_EntityManager.WorldUnmanaged, ids);
        
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
        def.AddComponentData(entity, context);
        
        Map.Layers.AddLayer<Map.Layers.Door>(entity, context);
        Map.Layers.AddLayer<Map.Layers.Floor>(entity, context);
        Map.Layers.AddLayer<Map.Layers.Structure>(entity, context);
        Map.Layers.AddLayer<Map.Layers.UserObject, Map.Layers.UserObject.Validator>(entity, context);

        var map = m_EntityManager.GetAspect<Map.Aspect>(entity);
        map.Init(ref system.CheckedStateRef, map);

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
