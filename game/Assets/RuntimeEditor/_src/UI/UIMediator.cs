using System;
using Common.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public abstract class UIMediator : MonoBehaviour
    {
        [SerializeField]
        private UIDocument m_Document;

        [SerializeField]
        VisualTreeAsset m_Template;

        [SerializeField]
        string m_Caption;

        private readonly DIContext.Var<IUIManager> m_Manager;
        public IUIManager UIManager => m_Manager.Value;

        protected VisualElement m_Parent;
        protected VisualElement m_Element;
        public VisualElement Element => m_Element;

        public string Caption => m_Caption;

        public void Initialize() => OnCreate();

        protected abstract VisualElement FindParent(VisualElement root);
        protected abstract void OnInitialize(VisualElement root);
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected void OnCreate()
        {
            m_Element = m_Template.Instantiate();
            m_Parent = FindParent(m_Document.rootVisualElement);
            m_Parent.Add(m_Element);
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
            return m_Template.Instantiate();
        }
    }
}