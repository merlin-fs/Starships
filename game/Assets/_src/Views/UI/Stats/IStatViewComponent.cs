using System;
using Unity.Transforms;

namespace Game.Views.Stats
{
    using Model.Stats;

    public interface IStatViewComponent
    {
        void Update(Stat stat, ITransformData transform);
    }
}