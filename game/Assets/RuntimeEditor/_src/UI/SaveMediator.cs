using System;
using UnityEngine;
using UnityEngine.UIElements;
using Game.Core.Saves;

namespace Game.UI
{
    public class SaveMediator : MonoBehaviour
    {
        [SerializeField] private UIDocument document;

        private readonly struct SavedContext: ISavedContext
        {
            public string Name { get; }
            private SavedContext(string name) => Name = name;

            public static implicit operator SavedContext(string name) => new SavedContext(name);
        }

        private void Awake()
        {
            document.rootVisualElement.Q<Button>("save")
                .RegisterCallback<ClickEvent>(e =>
                {
                    Save();
                });
            document.rootVisualElement.Q<Button>("load")
                .RegisterCallback<ClickEvent>(e =>
                {
                    Load();
                });
        }

        private void Load()
        {
            var manager = new SaveManager((SavedContext)"Test");
            manager.Load();
        }    
        private void Save()
        {
            var manager = new SaveManager((SavedContext)"Test");
            manager.Save();
        }
    }
}
