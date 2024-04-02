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
        private ObjectRepository Repository => Inject<ObjectRepository>.Value;
        
        public void ChoseGroup(string value)
        {
            var items = Repository.Find((item) => item.Labels.Contains(value));
            OnUpdateList?.Invoke(items);
        }
    }
}
