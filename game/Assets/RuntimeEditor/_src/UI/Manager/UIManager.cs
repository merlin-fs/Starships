using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    using Elements;

    public class UIManager: IUIManager
    {
        private UIDocument m_Document;
        private VisualElement m_Cancel;
        private List<VisualElement> m_PopupStack = new List<VisualElement>();

        public void Show(VisualElement element, ShowStyle style = ShowStyle.Normal)
        {
            switch (style)
            {
                case ShowStyle.Normal:
                    Show(element, true);
                    return;
                case ShowStyle.Popup:
                    ShowPopups(element);
                    return;
            }
        }

        public void Close(VisualElement element)
        {
            m_PopupStack.Remove(element);
            Show(element, false);
        }

        public void HidePopups()
        {
            foreach (var iter in m_PopupStack)
            {
                Show(m_Cancel, false);
                Show(iter, false);
            }
        }

        public void RestorePopups()
        {
            Show(m_Cancel, true);
            foreach (var iter in m_PopupStack)
                Show(iter, true);
        }

        private void ShowPopups(VisualElement element)
        {
            Show(m_Cancel, true);
            Show(element, true);
            if (!m_PopupStack.Contains(element))
                m_PopupStack.Add(element);
        }

        private void ClosePopups()
        {
            while (m_PopupStack.Count > 0)
            {
                var item = m_PopupStack.Last();
                m_PopupStack.RemoveAt(m_PopupStack.Count - 1);
                Show(m_Cancel, false);
                Show(item, false);
            }
        }

        public UIManager(GameObject root)
        {
            m_Document = root.GetComponent<UIDocument>();
            m_Cancel = root.GetComponent<RootClicked>()?.VisualElement;
            Show(m_Cancel, false);
            m_Document.rootVisualElement?.RegisterCallback<NavigationCancelEvent>(evt => OnCancel());
        }

        private void OnCancel()
        {
            ClosePopups();
        }

        private void Show(VisualElement element, bool show)
        {
            if (element == null)
                return;
            var style = show
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            element.style.display = style;
           /* 
            var style = show
                ? Visibility.Visible
                : Visibility.Hidden;
            
            if (element.style.visibility != style)
            {
                element.style.visibility = style;
                using (var change = ChangeEvent<Visibility>.GetPooled(element.style.visibility.value, style))
                {
                    change.target = element;
                    element.panel?.visualTree.SendEvent(change);
                }
            }
            */
        }
    }
}