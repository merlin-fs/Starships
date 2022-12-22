using Unity.Entities;
using UnityEngine;
using Game.Model;
using Common.Defs;
using Game.Model.Weapons;
using UnityEngine.UI;
using Unity.Collections;
using System.Xml;
using Game.Model.Stats;

public partial class EmptySystem : SystemBase
{
    protected override void OnUpdate() { }
}

public class TestPrefab : MonoBehaviour
{
    [SerializeField]
    public WeaponConfig weaponConfig;

    [SerializeField]
    Button m_BtnReload;

    private EntityManager m_EntityManager;

    private void Awake()
    {
        /*
        m_BtnReload.onClick.AddListener(
            () =>
            {
                
                var cmd = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>();
                var writer = cmd.CreateCommandBuffer().AsParallelWriter();
                var aspect = m_EntityManager.GetAspect<WeaponAspect>(m_Entity);
                aspect.Reload(writer, 0);
            });
        */
    }

    /*
    public class MyBaker : Baker<TestPrefab>
    {
        public override void Bake(TestPrefab authoring)
        {
            var entity = this.GetEntity();
            authoring.weaponConfig.Value.AddComponentData(entity, this);
            AddComponent<Prefab>();
        }
    }
    */

    private void Start()
    {
        Debug.LogError("1");
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var Archetype = m_EntityManager.CreateArchetype(
            ComponentType.ReadOnly<Prefab>(),
            ComponentType.ReadOnly<Modifier>()
            );

        using NativeArray<Entity> entities = m_EntityManager.CreateEntity(Archetype, 1500, Allocator.Temp);
        foreach(var entity in entities)
        {
            weaponConfig.Value.AddComponentData(entity, m_EntityManager);
        }
        //m_EntityManager.AddComponent<Prefab>(m_Entity);
    }
}
