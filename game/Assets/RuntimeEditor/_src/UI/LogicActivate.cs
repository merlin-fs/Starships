using Buildings;
using Common.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class LogicActivate : MonoBehaviour
    {
        readonly static DiContext.Var<IApiEditor> m_ApiEditor;
        
        [SerializeField] private UIDocument document;

        private void Awake()
        {
            var btn = document.rootVisualElement.Q<Toggle>("activate");
            btn.RegisterValueChangedCallback(e =>
            {
                m_ApiEditor.Value.SetLogicActive(e.newValue);
            });
        }
    }
}