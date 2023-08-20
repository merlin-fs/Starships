using System;
using Common.Core;
using Game.Core.Events;
using Game.Core.Repositories;
using Game.UI;
using Game.UI.Huds;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

using IEventHandler = Game.Core.Events.IEventHandler;

namespace Buildings
{
    public class BuildingContext : DiContext
    {
        [SerializeField] private Config config;
        [SerializeField] private UIDocument rootUI;
        [SerializeField] private Camera worldCamera;

        protected override void OnBind()
        {
            var api = new ApiEditor();
            Bind<IApiEditor>(api);
            Bind<IEventSender>(api.Events as IEventSender);
            Bind<IEventHandler>(api.Events);
            
            Bind<Config>(config);
            Bind<IUIManager>(new UIManager(rootUI.gameObject));
            Bind<IApiEditorHandler>(api as IApiEditorHandler);

            Bind<ObjectRepository>(new ObjectRepository());
            Bind<AnimationRepository>(new AnimationRepository());

            Bind<ReferenceSubSceneManager>(new ReferenceSubSceneManager());

            var doc = GetComponent<UIDocument>();
            Bind<HudManager>(new HudManager(doc, worldCamera));
        }
    }
}