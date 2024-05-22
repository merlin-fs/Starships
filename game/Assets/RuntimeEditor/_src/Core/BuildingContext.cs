using System;

using Common.UI;

using Game;
using Game.Core;
using Game.Core.Events;
using Game.Core.Spawns;
using Game.Core.Storages;
using Game.UI;
using Game.UI.Huds;
using Game.Views;

using Reflex.Core;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Buildings
{
    public class BuildingContext : MonoBehaviour, IInstaller
    {
        [SerializeField] private Config config;
        [SerializeField] private UIDocument rootUI;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ParticleManager particleManager;
        [SerializeField] private SpawnFactory spawnFactory;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(c => c.Construct<ApiEditor>(), typeof(IApiEditor));
            containerBuilder.AddSingleton<IEventSender>(c => (IEventSender)c.Resolve<IApiEditor>().Events);
            
            containerBuilder.AddSingleton(config);
            containerBuilder.AddSingleton<IStorage>(c => c.Construct<Storage>()); 

            containerBuilder.AddSingleton<IUIManager>(container =>
            {
                var manager = container.Construct<UIManager>(rootUI.gameObject, "main");
                //UI widgets
                manager.WithBindWidget(binder =>
                {
                    binder.Bind<GameUI>();
                    binder.Bind<LeftPanel>();
                });
                return manager;
            });

            containerBuilder.AddSingleton(c => c.Construct<RepositoryVfx>()); 
            containerBuilder.AddSingleton(particleManager);

            containerBuilder.AddSingleton(spawnFactory, typeof(Spawn.IFactory));
            containerBuilder.AddSingleton<Spawn.SpawnViewPool>(c => new Spawn.SpawnViewPool(transform, c));
            containerBuilder.AddSingleton<LogicApi>(c => new LogicApi());
            
                
            var doc = GetComponent<UIDocument>();
            containerBuilder.AddSingleton(new Hud.Manager(doc, worldCamera));
        }
    }
}