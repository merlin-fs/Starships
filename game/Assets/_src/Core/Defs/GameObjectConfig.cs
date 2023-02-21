using System;
using Unity.Entities;
using Common.Defs;
using Game.Model.Stats;
using Game.Model.Weapons;

namespace Game.Core.Defs
{
    public abstract class GameObjectConfig : ScriptableConfig
    {
        protected override void Configurate(Entity entity, IDefineableContext context)
        {
            context.SetName(entity, ID.ToString());

            if (this is IConfigStats stats)
            {
                var prepare = context.AddBuffer<PrepareStat>(entity);
                prepare.Add(new PrepareStat { ConfigID = ID });

                context.AddBuffer<Modifier>(entity);
                context.AddBuffer<Damage.LastDamage>(entity);
                var buff = context.AddBuffer<Stat>(entity);

                stats.Configurate(buff);
            }
        }
    }
}
