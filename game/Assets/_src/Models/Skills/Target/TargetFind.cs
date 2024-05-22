using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Model
{
    public partial struct Target
    {
        struct TempFindTarget
        {
            public Entity Entity;
            public float Magnitude;
        }

        public delegate bool Filter(float3 selfPosition, float3 targetPos);
        
        public static bool FindEnemy(Entity self, uint soughtTeams, Filter filter,
            NativeList<Entity> entities,
            ComponentLookup<LocalToWorld> transforms, 
            ComponentLookup<Team> teams, out Entity target)
        {
            TempFindTarget find = new() { Entity = Entity.Null, Magnitude = float.MaxValue };
            var selfPosition = transforms[self].Position;

            foreach (var candidate in entities)
            {
                var team = teams[candidate];
                if ((team.SelfTeam & soughtTeams) == 0)
                    continue;

                var targetPos = transforms[candidate].Position;
                var magnitude = (selfPosition - targetPos).magnitude();

                if (!(magnitude < find.Magnitude) || !filter(selfPosition, targetPos)) continue;
                
                find.Magnitude = magnitude;
                find.Entity = candidate;
            }

            target = find.Entity;
            return target != Entity.Null;
        }
    }
}
