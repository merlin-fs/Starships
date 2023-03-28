using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public class MainToolbarMediator : ToolbarMediator
    {
        [SerializeField]
        UIMediator[] m_Childs;

        private void Start()
        {
            Initialize();
            UIManager.Show(Element);
        }

        protected override void OnInitialize(VisualElement root)
        {
            base.OnInitialize(root);
            for (int i = 0; i < m_Childs.Length; i++)
            {
                m_Childs[i].Initialize();
            }
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
                var btn = (item as Toggle);
                btn.text = m_Childs[idx].Caption;

                btn.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                        UIManager.Show(m_Childs[idx].Element, ShowStyle.Popup);
                    else
                        UIManager.Close(m_Childs[idx].Element);
                });
                
            };

            itemsSource = m_Childs;
        }
    }
}