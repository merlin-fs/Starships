using Unity.Entities;
using UnityEngine;
using Common.Defs;
using Game.Model.Weapons;
using UnityEngine.UI;
using Unity.Collections;
using Game.Model.Stats;
using TMPro;
using Game.Model;
using Game.Model.Units;

public partial class EmptySystem : SystemBase
{
    protected override void OnUpdate() { }
}

public class TestPrefab : MonoBehaviour
{
    [SerializeField]
    TMP_Text m_Text;

    [SerializeField]
    public UnitConfig Config;

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

    private void CreateEntities(int count)
    {
        var Archetype = m_EntityManager.CreateArchetype(
            ComponentType.ReadOnly<Prefab>(),
            ComponentType.ReadOnly<Modifier>()
            );

        using NativeArray<Entity> entities = m_EntityManager.CreateEntity(Archetype, count, Allocator.Temp);
        foreach (var entity in entities)
        {
            Config.Value.AddComponentData(entity, m_EntityManager);
        }
    }

    private void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Config.Value.Weapon.Init();
        CreateEntities(1);
    }

    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
