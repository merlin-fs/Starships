using Buildings;
using Common.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI
{
    public class LogicActivate : MonoBehaviour
    {
        private static IApiEditor ApiEditor => Inject<IApiEditor>.Value;
        
        [SerializeField] private UIDocument document;

        private void Awake()
        {
            var btn = document.rootVisualElement.Q<Toggle>("activate");
            btn.RegisterValueChangedCallback(e =>
            {
                ApiEditor.SetLogicActive(e.newValue);
            });
        }
    }
}