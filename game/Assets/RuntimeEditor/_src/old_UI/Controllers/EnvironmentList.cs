using System;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;

using Reflex.Attributes;

namespace Game.UI.Elements
{
    using Core.Repositories;

    public class EnvironmentList: IUIController
    {
        public string BindName => "Environment";
        public event Action<IEnumerable<IConfig>> OnUpdateList;
        [Inject] private ObjectRepository m_Repository;
        
        public void ChoseGroup(string value)
        {
            var items = m_Repository.Find((item) => item.Labels.Contains(value));
            OnUpdateList?.Invoke(items);
        }
    }
}
