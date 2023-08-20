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
        //private int m_CancelRef = 0;


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
            ShowCancelButton(false);
            Show(element, false);
        }

        public void HidePopups()
        {
            foreach (var iter in m_PopupStack)
            {
                ShowCancelButton(false);
                Show(iter, false);
            }
        }

        public void RestorePopups()
        {
            ShowCancelButton(true);
            foreach (var iter in m_PopupStack)
                Show(iter, true);
        }

        private void ShowPopups(VisualElement element)
        {
            ShowCancelButton(true);
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
                ShowCancelButton(false);
                Show(item, false);
            }
        }

        private void ShowCancelButton(bool show)
        {
            /*
            if (show)
                Show(m_Cancel, ++m_CancelRef > 0);
            else
                Show(m_Cancel, --m_CancelRef > 0);
            if (m_CancelRef < 0) m_CancelRef = 0;
            */
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

            if (element.style.display != style)
            {
                using (var change = ChangeEvent<DisplayStyle>.GetPooled(element.style.display.value, style))
                {
                    change.target = element;
                    element.panel?.visualTree.SendEvent(change);
                }
                element.style.display = style;
            }
        }
    }
}