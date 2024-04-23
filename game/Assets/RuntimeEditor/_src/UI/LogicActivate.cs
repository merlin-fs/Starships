using System.Collections.Generic;

using Buildings;
using Common.UI;
using Reflex.Attributes;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class LogicActivate : UIWidget
    {
        [Inject] private IApiEditor m_ApiEditor;
        protected override void Bind()
        {
            Document.rootVisualElement.Q<Toggle>("activate")
                .RegisterValueChangedCallback(e =>
                {
                    m_ApiEditor.SetLogicActive(e.newValue);
                });
        }

        public override IEnumerable<VisualElement> GetElements()
        {
            yield return Document.rootVisualElement.Q<Toggle>("activate");
        }
    }
}