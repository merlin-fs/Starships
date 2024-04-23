﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reflex.Core;
using Reflex.Extensions;
using Reflex.Logging;
using Reflex.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly] // https://docs.unity3d.com/ScriptReference/Scripting.AlwaysLinkAssemblyAttribute.html

namespace Reflex.Injectors
{
    internal static class UnityInjector
    {
        internal static Action<Scene, SceneScope> OnSceneLoaded;
        internal static Container ProjectContainer { get; private set; }
        internal static Dictionary<Scene, Container> ContainersPerScene { get; } = new();
        internal static Dictionary<Scene, Action<ContainerBuilder>> ScenePreInstaller { get; } = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeAwakeOfFirstSceneOnly()
        {
            ReportReflexDebuggerStatus();
            
            ContainersPerScene.Clear();
            ProjectContainer = CreateProjectContainer();

            void InjectScene(Scene scene, SceneScope sceneScope)
            {
                ReflexLogger.Log($"Scene {scene.name} ({scene.GetHashCode()}) loaded", LogLevel.Development);
                var sceneContainer = CreateSceneContainer(scene, ProjectContainer, sceneScope);
                ContainersPerScene.Add(scene, sceneContainer);
                SceneInjector.Inject(scene, sceneContainer);
            }
            
            void DisposeScene(Scene scene)
            {
                ReflexLogger.Log($"Scene {scene.name} ({scene.GetHashCode()}) unloaded", LogLevel.Development);

                if (ContainersPerScene.Remove(scene, out var sceneContainer)) // Not all scenes has containers
                {
                    sceneContainer.Dispose();
                }
            }
            
            void DisposeProject()
            {
                ProjectContainer.Dispose();
                ProjectContainer = null;
                
                // Unsubscribe from static events ensuring that Reflex works with domain reloading set to false
                OnSceneLoaded -= InjectScene;
                SceneManager.sceneUnloaded -= DisposeScene;
                Application.quitting -= DisposeProject;
            }
            
            OnSceneLoaded += InjectScene;
            SceneManager.sceneUnloaded += DisposeScene;
            Application.quitting += DisposeProject;
        }

        private static Container CreateProjectContainer()
        {
            var builder = new ContainerBuilder().SetName("ProjectContainer");
            
            if (ResourcesUtilities.TryLoad<ProjectScope>(nameof(ProjectScope), out var projectScope))
            {
                builder.AddSingleton(InitializationManager.Instance, typeof(IInitialization));
                projectScope.InstallBindings(builder);
            }
            return builder.Build();
        }

        private static Container CreateSceneContainer(Scene scene, Container projectContainer, SceneScope sceneScope)
        {
            return projectContainer.Scope(builder =>
            {
                builder.SetName($"{scene.name} ({scene.GetHashCode()})");

                if (ScenePreInstaller.Remove(scene, out var preInstaller))
                {
                    preInstaller.Invoke(builder);
                }

                sceneScope.InstallBindings(builder);
            });
        }

        [Conditional("REFLEX_DEBUG")]
        private static void ReportReflexDebuggerStatus()
        {
            ReflexLogger.Log("Symbol REFLEX_DEBUG are defined, performance impacted!", LogLevel.Warning);
        }
    }
}
