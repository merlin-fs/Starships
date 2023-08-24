using System;
using Game.Model.Stats;
using Unity.Entities;

namespace Game.Model.Logics
{
    public struct Destroy : Logic.ISlice
    {
        public bool IsConditionHit(ref Logic.SliceContext context)
        {
            return context.Logic.IsCurrentAction(Global.Action.Destroy) && 
                   context.Logic.HasWorldState(Global.State.Dead, false);
        }

        public void Execute(ref Logic.SliceContext context)
        {
            UnityEngine.Debug.Log($"{context.Logic.Self} [Cleanup] set DeadTag");
            context.Writer.AddComponent<DeadTag>(context.Index, context.Logic.Self);
            context.Logic.SetWorldState(Global.State.Dead, true);
        }
    }
}