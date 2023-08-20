using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Game.UI.Elements
{
    public class MainToolbarMediator : ToolbarMediator
    {
        [SerializeField]
        private UIMediator[] children;

        private void Start()
        {
            Initialize();
            UIManager.Show(Element);
        }

        protected override void OnInitialize(VisualElement root)
        {
            base.OnInitialize(root);
            foreach (var child in children)
            {
                child.Initialize();
                child.Element.RegisterCallback<ChangeEvent<DisplayStyle>>(evt =>
                {
                    if (evt.newValue != DisplayStyle.None) return;
                    
                    foreach (var iter in Elements)
                        if (iter is Toggle {value: true} toggle)
                            toggle.SetValueWithoutNotify(false);
                });
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
                var btn = (Toggle)item;
                btn.text = children[idx].Caption;

                btn.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    if (evt.newValue)
                        UIManager.Show(children[idx].Element, ShowStyle.Popup);
                    else 
                        UIManager.Close(children[idx].Element);
                });
                
            };

            itemsSource = children;
        }
    }
}