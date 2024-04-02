using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public abstract class ToolbarMediator : UIMediator
    {
        private const string ROOT = "left_area";
        private const string TOOLBAR = "toolbar";

        private readonly List<VisualElement> m_Elements = new List<VisualElement>();
        protected IEnumerable<VisualElement> Elements => m_Elements;

        protected override VisualElement FindParent(VisualElement root)
        {
            return root.Q<VisualElement>(ROOT);
        }

        protected abstract void MakeItems(out Func<VisualElement> makeItem, out Action<VisualElement, int> bindItem, 
            out IList itemsSource);

        protected override void OnInitialize(VisualElement root)
        {
            var content = m_Element.Q<VisualElement>(TOOLBAR);
            content.Clear();
            MakeItems(out var makeItem, out var bindItem, out var itemsSource);
            m_Elements.Clear();
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
