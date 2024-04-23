using System.Collections.Generic;

using Common.UI;

using Game.Core.Storages;

using Reflex.Attributes;

using UnityEngine.UIElements;

namespace Game.UI
{
    public class SaveMediator : UIWidget
    {
        [Inject] private IStorage m_Storage;
        
        protected override void Bind()
        {
            Document.rootVisualElement.Q<Button>("save")
                .RegisterCallback<ClickEvent>(e =>
                {
                    Save();
                });
            Document.rootVisualElement.Q<Button>("load")
                .RegisterCallback<ClickEvent>(e =>
                {
                    Load();
                });
        }

        public override IEnumerable<VisualElement> GetElements()
        {
            yield return Document.rootVisualElement.Q<Button>("save");
            yield return Document.rootVisualElement.Q<Button>("load");
        }

        private readonly struct SavedContext: ISavedContext
        {
            public string Name { get; }
            private SavedContext(string name) => Name = name;

            public static implicit operator SavedContext(string name) => new SavedContext(name);
        }

        private void Load()
        {
            m_Storage.Load();
        }    

        private void Save()
        {
            m_Storage.Save();
        }
    }
}
