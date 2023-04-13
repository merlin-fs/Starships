using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Common.Defs;
using Buildings;
using Unity.Entities;
using Game.Core.Repositories;
using Game.Core.Prefabs;

namespace Game.UI.Elements
{
    public class EnvironmentMediatort: ToolbarMediator
    {
        const string k_List = "list";

        [SerializeField]
        VisualTreeAsset m_PopupTemplate;

        EnvironmentList m_List = new EnvironmentList();

        BuildingContext.Var<IApiEditor> m_ApiEditor;
        BuildingContext.Var<Repository> m_Repository;
        
        List<string> m_Items => m_Repository.Value.Labels.ToList();

        TemplateContainer m_Popup;
        ListView m_ListView;

        IConfig m_CurrentConfig;
        Entity m_CurrentEntity;

        protected override void MakeItems(out Func<VisualElement> makeItem, out Action<VisualElement, int> bindItem,
            out IList itemsSource)
        {
            makeItem = () => {
                var btn = new Toggle();
                return btn;
            };

            bindItem = (item, idx) =>
            {
                var btn = (item as Toggle);
                btn.text = m_Items[idx];

                btn.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                        ShowItem(m_Items[idx]);
                    else 
                        UIManager.Close(m_Popup);
                });
            };

            itemsSource = m_Items;
        }

        void ShowItem(string name)
        {
            m_List.ChoiseGroup(name);
            UIManager.Show(m_Popup, ShowStyle.Popup);
        }

        private void ChoiseItem(IConfig config)
        {
            UIManager.HidePopups();
            m_CurrentConfig = config;
            if (m_ApiEditor.Value.TryGetPlaceHolder(m_CurrentEntity, out IPlaceHolder holder))
                holder.Cancel();
            m_ApiEditor.Value.AddEnvironment(config);
        }

        protected override async void OnInitialize(VisualElement root)
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged;
            var system = manager.GetUnsafeSystemRef<PrefabEnvironmentSystem>(manager.GetExistingUnmanagedSystem<PrefabEnvironmentSystem>());
            await system.IsDone();

            base.OnInitialize(root);

            m_Popup = m_PopupTemplate.Instantiate();
            UIManager.Close(m_Popup);
            root.Add(m_Popup);

            m_Popup.RegisterCallback<ChangeEvent<DisplayStyle>>(evt =>
            {
                if (evt.newValue == DisplayStyle.None)
                {
                    foreach (var iter in Elements)
                        if (iter is Toggle toggle && toggle.value)
                            toggle.SetValueWithoutNotify(false);
                }
            });


            m_ListView = m_Popup.Q<ListView>(k_List);
            m_ListView.itemsChosen += items =>
            {
                var obj = (IConfig)items.First();
                ChoiseItem(obj);
            };

            m_List.OnUpdateList += (IEnumerable<IConfig> configs) =>
            {
                m_ListView.Clear();
                var list = configs.ToList();

                m_ListView.makeItem = () => new Label();
                m_ListView.bindItem = (item, idx) =>
                {
                    if (idx >= list.Count)
                        return;
                    if (item is Label label)
                    {
                        label.text = list[idx].ID.ToString();
                    }
                };
                
                m_ListView.itemsSource = list;
            };

            m_ApiEditor.Value.Events.RegisterCallback<EventPlace>(evt =>
            {
                switch (evt.State)
                {
                    case EventPlace.eState.New:
                        m_CurrentEntity = evt.Entity;
                        break;
                    case EventPlace.eState.Cancel:
                        m_CurrentEntity = Entity.Null;
                        UIManager.RestorePopups();
                        break;
                    case EventPlace.eState.Apply:
                        m_CurrentEntity = Entity.Null;
                        m_ApiEditor.Value.AddEnvironment(m_CurrentConfig);
                        break;
                }
            });
        }
    }
}
