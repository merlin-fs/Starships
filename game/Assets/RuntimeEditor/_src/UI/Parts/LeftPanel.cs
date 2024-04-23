
using System;
using System.Collections.Generic;

using Common.UI;

namespace Game.UI
{
    public class LeftPanel : UIWidgetContainer
    {
        public override IEnumerable<Type> GetWidgetTypes()
        {
            yield return typeof(ToolbarEnvironmentMediator);
        }
    }
}
