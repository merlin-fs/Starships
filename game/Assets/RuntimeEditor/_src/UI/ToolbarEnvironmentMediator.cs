using System;
using System.Collections.Generic;
using System.Linq;

using Buildings;

using UnityEngine;
using UnityEngine.UIElements;

using Common.Core;
using Common.Defs;

using Game.Core.Events;
using Game.Core.Repositories;
using Game.Model.Worlds;

using Unity.Entities;

using IEventHandler = Game.Core.Events.IEventHandler;

namespace Game.UI
{
    public class ToolbarEnvironmentMediator : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private static ObjectRepository Repository => Inject<ObjectRepository>.Value;
        private static IEventHandler Handler => Inject<IEventHandler>.Value;
        private static IApiEditor ApiEditor => Inject<IApiEditor>.Value;
        
        private VisualElement m_Content;
        private ListView m_ListView;
        private List<IConfig> m_CurrentList;
        
        private IConfig m_CurrentConfig;
        private Entity m_CurrentEntity;
        
        private void Awake()
        {
            Handler.RegisterCallback<EventRepository>(OnEventRepository);
            m_Content = document.rootVisualElement.Q<VisualElement>("environment");
            m_ListView = document.rootVisualElement.Q<ListView>("object_list");
            BindItems();
        }

        private void BindItems()
        {
            m_Content.Clear();
            m_ListView.bindItem = (item, idx) =>
            {
                if (idx >= m_CurrentList.Count) return;
                ((Label)item).text = m_CurrentList[idx].ID.ToString();
            };
            m_ListView.makeItem = () => new Label();
            m_ListView.itemsChosen += items =>
            {
                var obj = (IConfig)items.First();
                ChoseItem(obj);
            };
            
            ApiEditor.Events.RegisterCallback<EventPlace>(evt =>
            {
                switch (evt.Value)
                {
                    case EventPlace.State.New:
                        m_CurrentEntity = evt.Entity;
                        break;
                    case EventPlace.State.Cancel:
                        m_CurrentEntity = Entity.Null;
                        break;
                    case EventPlace.State.Apply:
                        m_CurrentEntity = Entity.Null;
                        ApiEditor.AddEnvironment(m_CurrentConfig);
                        break;
                }
            });
        } 

        private void ChoseItem(IConfig config)
        {
            m_ListView.style.display = DisplayStyle.None;
            m_CurrentConfig = config;
            if (ApiEditor.TryGetPlaceHolder(m_CurrentEntity, out IPlaceHolder holder))
                holder.Cancel();
            ApiEditor.AddEnvironment(config);
        }

        private void OnDestroy()
        {
            Handler.UnregisterCallback<EventRepository>(OnEventRepository);
        }

        private void OnEventRepository(EventRepository evt)
        {
            if (evt.Repository != Repository || evt.State != EventRepository.Enum.Done) return;
            BuildList();
        }

        private void BuildList()
        {
            m_Content.Clear();
            foreach (var iter in Map.Layers.Values)
            {
                var btn = new RadioButton(iter.SelfType.Name);
                m_Content.Add(btn);
                btn.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                        ShowListObject(iter.TypeIndex);
                });
            }
        }

        private void ShowListObject(TypeIndex typeIndex)
        {
            m_ListView.Clear();
            m_CurrentList = Repository.Find(iter =>
            {
                if (iter.Entity.Prefab == Entity.Null) return false;
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (!manager.HasComponent<Map.Placement>(iter.Entity.Prefab)) return false;
                var placement = manager.GetComponentData<Map.Placement>(iter.Entity.Prefab);
                return placement.Value.Layer == typeIndex;
            }).ToList();
            m_ListView.style.display = DisplayStyle.Flex;
            m_ListView.itemsSource = m_CurrentList;
        }
    }
}
