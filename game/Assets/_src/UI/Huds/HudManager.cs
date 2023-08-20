using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Game.UI.Huds
{
    public class HudManager
    {
        private readonly UIDocument m_UIDocument;
        private readonly Dictionary<Type, HudConfig> m_Configs = new Dictionary<Type, HudConfig>();
        public Camera WorldCamera { get; }

        public HudManager(UIDocument uiDocument, Camera worldCamera)
        {
            m_UIDocument = uiDocument;
            WorldCamera = worldCamera;
            LoadConfigs();
        }

        public T GetHud<T>()
            where T : IHud
        {
            var config = m_Configs[typeof(T)];
            var hud = (T)Activator.CreateInstance(config.Hud);
            var element = config.Template.Instantiate();
            foreach (var style in config.Styles)
            {
                element.styleSheets.Add(style);
            }
            m_UIDocument.rootVisualElement.Add(element);
            hud.Initialize(this, element);
            return hud;
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
        
        public void ReleaseHud<T>(T hud)
            where T : IHud
        {
            //hud.parent.Remove(hud);
        }
    }
}