using System;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;
using Common.Repositories;

namespace Game.UI.Elements
{
    using Buildings.Environments;
    using Core.Repositories;

    using Game.Core.Prefabs;

    using Unity.Entities;

    public class EnvironmentList: IUIController
    {
        public string BindName => "Environment";

        public event Action<IEnumerable<IConfig>> OnUpdateList;

        private IRepository<ObjectID, IConfig> m_Repository;
        
        public IRepository<ObjectID, IConfig> Current => m_Repository;

        public void ChoiseGroup(string value)
        {
            m_Repository = Repositories.Instance.GetRepo(value);
            OnUpdateList?.Invoke(m_Repository.Find());
        }
    }
}
