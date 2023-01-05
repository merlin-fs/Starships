using System;
using Unity.Entities;
using Common.Defs;
using Game.Model.Stats;

namespace Game.Core.Defs
{
    public abstract class GameObjectConfig : ScriptableConfig
    {
        protected override void Configurate(Entity entity, IDefineableContext context)
        {
            if (this is IConfigStats stats)
            {
                var prepare = context.AddBuffer<PrepareStat>(entity);
                prepare.Add(new PrepareStat { ConfigID = ID });
                context.AddBuffer<Modifier>(entity);
                var buff = context.AddBuffer<Stat>(entity);
                stats.Configurate(buff);
            }
        }
    }
}
