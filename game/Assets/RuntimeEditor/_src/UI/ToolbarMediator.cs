using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public abstract class ToolbarMediator : UIMediator
    {
        const string k_Root = "main";
        protected const string k_Toolbar = "toolbar";

        private List<VisualElement> m_Elements = new List<VisualElement>();
        protected IReadOnlyList<VisualElement> Elements => m_Elements;

        protected override VisualElement FindParent(VisualElement root)
        {
            return root.Q<VisualElement>(k_Root);
        }

        protected abstract void MakeItems(out Func<VisualElement> makeItem, out Action<VisualElement, int> bindItem, 
            out IList itemsSource);

        protected override void OnInitialize(VisualElement root)
        {
            UIManager.Close(m_Element);
            var content = m_Element.Q<VisualElement>(k_Toolbar);
            MakeItems(out var makeItem, out var bindItem, out var itemsSource);
            m_Elements.Capacity = itemsSource.Count;
            for (var i = 0; i < itemsSource.Count; i++)
            {
                var item = makeItem();
                content.Add(item);
                bindItem(item, i);
                m_Elements.Add(item);
            }
        }
    }
}
