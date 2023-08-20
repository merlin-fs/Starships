using System;
using Common.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public abstract class UIMediator : MonoBehaviour
    {
        [SerializeField] private UIDocument document;
        [SerializeField] private VisualTreeAsset template;
        [SerializeField] private StyleSheet style;
        [SerializeField] private string caption;

        private readonly DiContext.Var<IUIManager> m_Manager;
        public IUIManager UIManager => m_Manager.Value;

        protected VisualElement m_Parent;
        protected VisualElement m_Element;
        public VisualElement Element => m_Element;

        public string Caption => caption;

        public void Initialize() => OnCreate();

        protected abstract VisualElement FindParent(VisualElement root);
        protected abstract void OnInitialize(VisualElement root);
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        private void OnCreate()
        {
            m_Element = GetTemplateContainer();
            if (style != null)
                m_Element.styleSheets.Add(style);
            m_Parent = FindParent(document.rootVisualElement);
            m_Parent.Add(m_Element);
            UIManager.Close(m_Element);
            OnInitialize(m_Parent);

            Element.RegisterCallback<ChangeEvent<DisplayStyle>>(evt =>
            {
                switch (evt.newValue)
                {
                    case DisplayStyle.None:
                        OnHide();
                        break;
                    case DisplayStyle.Flex:
                        OnShow();
                        break;
                }
            });
        }

        protected virtual TemplateContainer GetTemplateContainer()
        {
            return template.Instantiate();
        }
    }
}