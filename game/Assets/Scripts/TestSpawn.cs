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

public class TestSpawn : MonoBehaviour
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
        var ecb = m_EntityManager.World.GetExistingSystemManaged<GameSpawnSystemCommandBufferSystem>()
            .CreateCommandBuffer();


        if (PrefabStore.Instance.TryGet("Unit", out Entity prefab))
            for (var i = 0; i < count; i++)
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, new SpawnTag() { Entity = prefab });
            }
    }

    private async void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var guid = SceneSystem.GetSceneGUID(ref m_EntityManager.WorldUnmanaged.GetExistingSystemState<SceneSystem>(), "Assets/Scenes/SampleScene/New Sub Scene.unity");
        //var loadParameters = new SceneSystem.LoadParameters { Flags = SceneLoadFlags.LoadAdditive | SceneLoadFlags.NewInstance };
        var sceneEntity = SceneSystem.LoadSceneAsync(m_EntityManager.WorldUnmanaged, guid);
        //SceneSystem.LoadPrefabAsync(m_EntityManager.WorldUnmanaged, guid);
        await Repositories.Instance.ConfigsAsync();
        
        //var store = SystemAPI.GetSingletonEntity<Game.Core.Prefabs.PrefabData>();
        //SystemAPI.SetBufferEnabled<Game.Core.Prefabs.PrefabData>(store, true);
    }

    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
