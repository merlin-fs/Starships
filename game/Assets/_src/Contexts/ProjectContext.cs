using Common.Core.Loading;
using Game.Core.Loading;
using Game.Core.Repositories;
using Reflex.Core;
using Reflex.Injectors;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Game.Core
{
    public class ProjectContext : MonoBehaviour, IInstaller
    {
        [SerializeField] private string sceneName;
        [SerializeField] private LoadingConfig loadingConfig;
        
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            containerBuilder.AddSingleton(c => c.Construct<ObjectRepository>());

            LoadGame(containerBuilder);
        }

        private void LoadGame(ContainerBuilder containerBuilder)
        {
            ILoadingManager loadingManager = new LoadingManager(loadingConfig.GetCommands());
            containerBuilder.AddSingleton(loadingManager, typeof(ILoadingManager));
            AttributeInjector.Inject(loadingManager, containerBuilder.Build());

            loadingManager.Start();
        }
    }
}