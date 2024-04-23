using UnityEngine;

namespace Game.Core.Storages
{
    public interface IStorage
    {
        void Load();
        void Save();
    }
}
