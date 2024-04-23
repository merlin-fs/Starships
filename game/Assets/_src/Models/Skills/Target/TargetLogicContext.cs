using System;
using Game.Core;
using Unity.Collections.LowLevel.Unsafe;
using Game.Model.Logics;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Game.Model
{
    public partial struct Target
    {
        public readonly struct Context: Logic.ILogicContext<Target>
        {
            public LogicHandle LogicHandle => LogicHandle.From<Target>();
        }
    }
}