using System;
using Common.Core;

using Game.Core.Events;
using Game.UI;

using UnityEngine;
using UnityEngine.UIElements;

namespace Buildings
{
    public class BuildingContext : DIContext
    {
        [SerializeField]
        private Config m_Config;

        [SerializeField]
        private UIDocument m_RootUI;

        protected override void OnBind()
        {
            var api = new ApiEditor();
            Bind<IApiEditor>(api);
            Bind<IEventSender>(api.Events as IEventSender);
            Bind<Config>(m_Config);
            Bind<IUIManager>(new UIManager(m_RootUI.gameObject));
        }
    }
}