using System;
using System.Collections.Generic;
using System.Linq;

using Buildings;
using UnityEngine.UIElements;

using Common.Defs;
using Common.UI;

using Game.Core.Repositories;
using Game.Model.Worlds;

using Reflex.Attributes;

using Unity.Entities;

namespace Game.UI
{
    public class ToolbarEnvironmentMediator : UIWidget
    {
        [Inject] private ObjectRepository m_Repository; 
        [Inject] private IApiEditor m_ApiEditor;
        [Inject] private IUIManager m_UIManager;
        
        private VisualElement m_Content;
        private ListView m_ListView;
        private List<IConfig> m_CurrentList;
        
        private IConfig m_CurrentConfig;
        private IPlaceHolder m_CurrentObject;
        
        protected override void Bind()
        {
            m_Content = Document.rootVisualElement.Q<VisualElement>("environment");
            m_ListView = Document.rootVisualElement.Q<ListView>("object_list");
            //Document.rootVisualElement?.RegisterCallback<NavigationCancelEvent>(evt => OnCancel());
            m_Content.RegisterCallback<NavigationCancelEvent>(evt => OnCancel());
            m_ListView.RegisterCallback<NavigationCancelEvent>(evt => OnCancel());
            BindItems();
            BuildList();
        }

        public override IEnumerable<VisualElement> GetElements()
        {
            yield return m_Content;
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
            
            m_ApiEditor.Events.RegisterCallback<EventPlace>(evt =>
            {
                switch (evt.Value)
                {
                    case EventPlace.State.New:
                        //m_CurrentEntity = evt.Entity;
                        break;
                    case EventPlace.State.Cancel:
                        //m_CurrentEntity = Entity.Null;
                        break;
                    case EventPlace.State.Apply:
                        //m_CurrentEntity = Entity.Null;
                        m_CurrentObject = m_ApiEditor.AddObject(m_CurrentConfig);
                        break;
                }
            });
        } 

        private void ChoseItem(IConfig config)
        {
            m_ListView.style.display = DisplayStyle.None;
            m_CurrentConfig = config;
            m_ApiEditor.Remove(m_CurrentObject); 
            m_CurrentObject = m_ApiEditor.AddObject(config);
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

        private void OnCancel()
        {
            m_UIManager.ShowCancelButton(false);
            foreach (var item in m_Content.Query<RadioButton>().ToList())
            {
                item.value = false;
            }
            m_ListView.style.display = DisplayStyle.None;
        }
            
        private void ShowListObject(TypeIndex typeIndex)
        {
            m_UIManager.ShowCancelButton(false);
            m_UIManager.ShowCancelButton(true);
            m_ListView.Clear();
            m_CurrentList = m_Repository.Find(iter =>
            {
                if (iter.Entity.EntityPrefab == Entity.Null) return false;
                var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (!manager.HasComponent<Map.Placement>(iter.Entity.EntityPrefab)) return false;
                
                var placement = manager.GetComponentData<Map.Placement>(iter.Entity.EntityPrefab);
                return placement.Value.Layer == typeIndex;
            }).ToList();
            m_ListView.style.display = DisplayStyle.Flex;
            m_ListView.itemsSource = m_CurrentList;
        }
    }
}
