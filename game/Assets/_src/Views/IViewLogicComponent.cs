using Game.Core;

using UnityEngine;

namespace Game.Views
{
    public interface IViewLogicComponent: IViewComponent
    {
        void ChangeAction(LogicActionHandle action);
    }
}
