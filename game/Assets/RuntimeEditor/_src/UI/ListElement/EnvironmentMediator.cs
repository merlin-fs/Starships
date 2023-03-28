using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Common.Defs;
using Buildings;

namespace Game.UI.Elements
{
    public class EnvironmentMediatort: ToolbarMediator
    {
        const string k_List = "list";

        [SerializeField]
        VisualTreeAsset m_PopupTemplate;

        EnvironmentList m_List = new EnvironmentList();

        BuildingContext.Var<IApiEditor> m_ApiEditor;
        IApiEditor ApiEditor => m_ApiEditor.Value;

        TemplateContainer m_Popup;
        ListView m_ListView;

        List<string> m_Items = new List<string>() { "Floor" };

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

        protected override void OnHide()
        {
            base.OnHide();
            foreach (var iter in Elements)
                if (iter is Toggle toggle)
                    toggle.value = false;
        }

        private void ChoiseItem(IConfig config)
        {
            ApiEditor.AddEnvironment(config);
            /*
            ApiEditor.Events.RegisterCallback<PlaceEvent>(async evt =>
            {
                switch (evt.State)
                {
                    case PlaceEvent.eState.Cancel:
                        UIManager.RestorePopups();
                        break;
                    case PlaceEvent.eState.Apply:
                        await ApiEditor.AddEnvironment(config);
                        break;
                }
            });
            */
        }

        protected override void OnInitialize(VisualElement root)
        {
            base.OnInitialize(root);
            
            m_Popup = m_PopupTemplate.Instantiate();
            UIManager.Close(m_Popup);
            root.Add(m_Popup);

            m_ListView = m_Popup.Q<ListView>(k_List);
            m_ListView.itemsChosen += items =>
            //m_ListView.selectedIndicesChanged += (items) =>
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
                    if (item is Label label)
                    {
                        label.text = list[idx].ID.ToString();
                    }
                };
                
                m_ListView.itemsSource = list;
            };
            
        }
    }
}
