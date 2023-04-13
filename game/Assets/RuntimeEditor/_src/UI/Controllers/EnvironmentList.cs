using System;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;

namespace Game.UI.Elements
{
    using Core.Repositories;

    public class EnvironmentList: IUIController
    {
        public string BindName => "Environment";

        public event Action<IEnumerable<IConfig>> OnUpdateList;

        readonly DIContext.Var<Repository> m_Repository;

        public void ChoiseGroup(string value)
        {
            var items = m_Repository.Value.Find((item) => item.Labels.Contains(value));
            OnUpdateList?.Invoke(items);
        }
    }
}
