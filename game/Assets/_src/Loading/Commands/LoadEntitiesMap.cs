using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Core.Loading;
using Common.Defs;

using Cysharp.Threading.Tasks;

using Game.Model.Worlds;

using Reflex.Attributes;
using Reflex.Core;
using Reflex.Extensions;
using Reflex.Injectors;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine.SceneManagement;

namespace Game
{
    public class LoadEntitiesMap : ILoadingCommand
    {
        public float GetProgress()
        {
            return 1;
        }

        public Task Execute(ILoadingManager manager)
        {
            return UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                var container = SceneManager.GetActiveScene().GetSceneContainer();
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                var def = new Map.Data.Def
                {
                    Size = new int2(100, 100)
                };

                var entity = entityManager.CreateSingleton<Map.Data>();
                var context = new EntityManagerContext(entityManager);
                def.AddComponentData(entity, context);
        
                Map.Layers.AddLayer<Map.Layers.Door>(entity, context);
                Map.Layers.AddLayer<Map.Layers.Floor>(entity, context);
                Map.Layers.AddLayer<Map.Layers.Structure>(entity, context);
                Map.Layers.AddLayer<Map.Layers.UserObject, Map.Layers.UserObject.Validator>(entity, context);

                var map = entityManager.GetAspect<Map.Aspect>(entity);
                var system = entityManager.World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();
                map.Init(ref system.CheckedStateRef, map);
            }).AsTask();
        }
    }
}
