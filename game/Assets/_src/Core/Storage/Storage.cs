using Reflex.Attributes;
using Reflex.Core;

using UnityEngine;

namespace Game.Core.Storages
{
    public class Storage : IStorage
    {
        [Inject] private Container m_Container;
        
        public void Load()
        {
            var manager = m_Container.Construct<SaveManager>((SavedContext)"Test");
            manager.Load();
        }

        public void Save()
        {
            var manager = m_Container.Construct<SaveManager>((SavedContext)"Test");
            manager.Save();
        }
        
        private readonly struct SavedContext: ISavedContext
        {
            public string Name { get; }

            private SavedContext(string name) => Name = name;

            public static implicit operator SavedContext(string name) => new SavedContext(name);
        }
    }
}
