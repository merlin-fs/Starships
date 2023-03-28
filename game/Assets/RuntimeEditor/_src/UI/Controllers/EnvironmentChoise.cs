using System;
using Unity.Entities;
using Common.Defs;
using Buildings.Environments;

namespace Game.UI.Elements
{
    using Core.Prefabs;

    public class EnvironmentChoise : IUIController
    {
        public string BindName => "Environment";

        public event Action<IConfig> OnChoise;

        public void Choise(IConfig value)
        {
            AddItem(value);
        }

        private void AddItem(IConfig config)
        {
            OnChoise?.Invoke(config);
        }
    }
}
