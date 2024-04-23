using System;
using System.Collections.Generic;
using Common.UI;

namespace Game.UI
{
    public class GameUI : UIWidgetContainer
    {
        public override IEnumerable<Type> GetWidgetTypes()
        {
            yield return typeof(LogicActivate);
            yield return typeof(SaveMediator);
            yield return typeof(ToolbarEnvironmentMediator);
        }
    }
}
