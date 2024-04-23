using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Common.Defs;
using Buildings;

using Common.Core;

using Unity.Entities;
using Game.Model.Worlds;

using Reflex.Attributes;

namespace Game.UI.Elements
{
    public class LayerMediator: ToolbarMediator
    {
        const string LIST = "list";

        private readonly EnvironmentList m_List = new EnvironmentList();

        [Inject] private IApiEditor m_ApiEditor;
        private List<string> Items => Map.Layers.Values.Select(iter => iter.Type.Name).ToList();
        private TemplateContainer m_Popup;
        private ListView m_ListView;
        private IConfig m_CurrentConfig;
        private Entity m_CurrentEntity;

        protected override void OnShow()
        {
            base.OnInitialize(m_Parent);
        }
        
        protected override VisualElement FindParent(VisualElement root)
        {
            return root.Q<VisualElement>("bottom_area");
        }
        
        protected override void MakeItems(out Func<VisualElement> makeItem, out Action<VisualElement, int> bindItem,
            out IList itemsSource)
        {
            makeItem = () => {
                var btn = new Toggle();
                return btn;
            };

            bindItem = (item, idx) =>
            {
                var btn = (Toggle)item;
                btn.text = Items[idx];

                btn.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    /*
                    if (evt.newValue)
                        ShowItem(Items[idx]);
                    else 
                        m_UIManager.Close(m_Popup);
                        */
                });
            };

            itemsSource = Items;
        }

        void ShowItem(string label)
        {
            m_List.ChoseGroup(label);
            //m_UIManager.Show(m_Popup, ShowStyle.Popup);
        }

        private void ChoseItem(IConfig config)
        {
            //m_UIManager.HidePopups();
            m_CurrentConfig = config;
            if (m_ApiEditor.TryGetPlaceHolder(m_CurrentEntity, out IPlaceHolder holder))
                holder.Remove();
            m_ApiEditor.AddEnvironment(config);
        }

        protected override void OnInitialize(VisualElement root)
        {
            //m_Element.
            //base.OnInitialize(root);
            /*
            m_Popup = popupTemplate.Instantiate();
            UIManager.Close(m_Popup);
            root.Add(m_Popup);

            m_Popup.RegisterCallback<ChangeEvent<DisplayStyle>>(evt =>
            {
                if (evt.newValue != DisplayStyle.None) return;
                
                foreach (var iter in Elements)
                    if (iter is Toggle {value: true} toggle)
                        toggle.SetValueWithoutNotify(false);
            });


            m_ListView = m_Popup.Q<ListView>(LIST);
            m_ListView.itemsChosen += items =>
            {
                var obj = (IConfig)items.First();
                ChoseItem(obj);
            };

            m_List.OnUpdateList += (configs) =>
            {
                m_ListView.Clear();
                var list = configs.ToList();

                m_ListView.makeItem = () => new Label();
                m_ListView.bindItem = (item, idx) =>
                {
                    if (item is Label label)
                    {
                        label.text = list[idx].ID.ToString();
                    }
                };
                
                m_ListView.itemsSource = list;
            };

            m_ApiEditor.Value.Events.RegisterCallback<EventPlace>(evt =>
            {
                switch (evt.Value)
                {
                    case EventPlace.State.New:
                        m_CurrentEntity = evt.Entity;
                        break;
                    case EventPlace.State.Cancel:
                        m_CurrentEntity = Entity.Null;
                        UIManager.RestorePopups();
                        break;
                    case EventPlace.State.Apply:
                        m_CurrentEntity = Entity.Null;
                        m_ApiEditor.Value.AddEnvironment(m_CurrentConfig);
                        break;
                }
            });
            */
        }
    }
}