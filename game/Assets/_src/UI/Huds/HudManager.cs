using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Common.Core;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public abstract partial class Hud
    {
        public class Manager: IInjectionInitable
        {
            private readonly UIDocument m_UIDocument;
            private readonly Dictionary<Type, HudConfig> m_Configs = new Dictionary<Type, HudConfig>();
            private VisualElement Root => m_UIDocument.rootVisualElement;
            public Camera WorldCamera { get; }

            private readonly ConcurrentQueue<Command> m_InstallCommands = new ConcurrentQueue<Command>();
            private readonly ConcurrentQueue<Command> m_RemoveCommands = new ConcurrentQueue<Command>();
            private class Command
            {
                public HudConfig Config;
                public Hud Hud;
            }
            
            void IInjectionInitable.Init(IDiContext context)
            {
                if (!m_UIDocument) return;
                //Install
                Root.schedule.Execute(() =>
                {
                    if (!m_InstallCommands.TryDequeue(out var cmd)) return;
                    var element = cmd.Config.Template.Instantiate();
                    foreach (var style in cmd.Config.Styles)
                    {
                        element.styleSheets.Add(style);
                    }
                    cmd.Hud.Configure(element);
                    Root.Add(element);
                }).When(() => !m_InstallCommands.IsEmpty).EveryFrame();

                //Remove
                Root.schedule.Execute(() =>
                {
                    if (!m_RemoveCommands.TryDequeue(out var cmd)) return;
                    Root.Remove(cmd.Hud.Element);
                }).When(() => !m_RemoveCommands.IsEmpty).EveryFrame();
            }

            public Manager(UIDocument uiDocument, Camera worldCamera)
            {
                m_UIDocument = uiDocument;
                WorldCamera = worldCamera;
                LoadConfigs();
            }

            public T GetHud<T>()
                where T : Hud
            {
                var config = m_Configs[typeof(T)];
                var hud = (T)Activator.CreateInstance(config.Hud);
                hud.Initialize(this);
                m_InstallCommands.Enqueue(new Command 
                {
                    Config = config,
                    Hud = hud,
                });
                return hud;
            }

            public void ReleaseHud<T>(T hud)
                where T : Hud
            {
                m_RemoveCommands.Enqueue(new Command 
                {
                    Hud = hud,
                });
            }

            private async void LoadConfigs()
            {
                await Addressables.LoadAssetsAsync<HudConfig>("Huds", null).Task
                    .ContinueWith(task =>
                    {
                        foreach (var iter in task.Result)
                        {
                            m_Configs.Add(iter.Hud, iter);
                        }
                    });
            }
        }
    }
}