using System;
using Unity.Transforms;
using Game.Model.Stats;

namespace Game.Views.Stats
{
    public interface IStatViewComponent : IViewComponent, IDisposable
    {
        void Update(in StatAspect stat, in LocalTransform transform);
    }
}