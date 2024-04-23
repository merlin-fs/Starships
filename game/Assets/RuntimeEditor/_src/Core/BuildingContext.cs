using System;

using Common.UI;

using Game.Core.Events;
using Game.Core.Spawns;
using Game.Core.Storages;
using Game.UI;
using Game.UI.Huds;
using Game.Views;

using Reflex.Core;
using Reflex.Injectors;

using UnityEngine;
using UnityEngine.UIElements;

using IEventHandler = Game.Core.Events.IEventHandler;

namespace Buildings
{
    public class BuildingContext : MonoBehaviour, IInstaller
    {
        [SerializeField] private Config config;
        [SerializeField] private UIDocument rootUI;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ParticleManager particleManager;

        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            containerBuilder.AddSingleton(c => c.Construct<ApiEditor>(), typeof(IApiEditor), typeof(IApiEditorHandler));
            
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
            
            containerBuilder.AddSingleton(particleManager);

            containerBuilder.AddSingleton<Spawn.SpawnViewPool>(c => new Spawn.SpawnViewPool(transform, c));
                
            var doc = GetComponent<UIDocument>();
            containerBuilder.AddSingleton(new Hud.Manager(doc, worldCamera));
        }
    }
}