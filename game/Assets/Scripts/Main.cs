using System;
using UnityEngine;
using UnityEngine.UI;

/*
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class GameSpawnSystemGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(GameSpawnSystemGroup), OrderFirst = true)]
public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }
*/

public class Main : MonoBehaviour
{
    [SerializeField]
    Button m_SpawnButton;

    void Start()
    {
        m_SpawnButton.onClick.AddListener(Spawn);

    }

    private void Spawn()
    {
        /*
        var mgr = World.DefaultGameObjectInjectionWorld.EntityManager;
        var buff = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
        var writer = buff.CreateCommandBuffer().AsParallelWriter();
        */
    }
}
