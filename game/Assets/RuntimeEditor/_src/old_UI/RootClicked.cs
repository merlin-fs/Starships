using System;
using Common.Core;

using Game.Model;

using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public class RootClicked : MonoBehaviour
    {
        [SerializeField]
        private UIDocument m_Document;

        [SerializeField]
        private string m_RootName;
        
        private VisualElement m_Element;
        public VisualElement VisualElement => m_Element;
        public IUIManager UIManager => Inject<IUIManager>.Value;

        private void OnClick()
        {
            using (var cancel = NavigationCancelEvent.GetPooled())
            {
                m_Element.panel.visualTree.SendEvent(cancel);
            }
        }

        private void Awake()
        {
            var root = m_Document.rootVisualElement.Q(m_RootName);
            m_Element = new Button(OnClick);
            m_Element.style.position = Position.Absolute;
            m_Element.style.top = 0;
            m_Element.style.left = 0;
            m_Element.style.flexGrow = new StyleFloat(1);
            m_Element.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            m_Element.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            m_Element.style.opacity = new StyleFloat(0f);
            root.Insert(0, m_Element);
        }
    }
}