using Game.Core.Prefabs;
using Game.Model.Worlds;
using Game.Views;

using Unity.Entities;
using Unity.Transforms;

using UnityEngine;

namespace Game.Model
{
    public partial struct Move
    {
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial struct MoveHybridSystem : ISystem
        {
            private EntityQuery m_Query;

            public void OnCreate(ref SystemState state)
            {
                m_Query = SystemAPI.QueryBuilder()
                    .WithAll<LocalTransform, PrefabInfo.ContextReference>()
                    .Build();
                m_Query.SetChangedVersionFilter(ComponentType.ReadOnly<LocalTransform>());
                state.RequireForUpdate(m_Query);
            }

            public void OnUpdate(ref SystemState state)
            {
                foreach (var (transform, context) in SystemAPI.Query<RefRO<LocalTransform>, PrefabInfo.ContextReference>()
                             .WithChangeFilter<LocalTransform>())
                {
                    var view = context.Value.Resolve<IView>();
                    view.Transform.localPosition = transform.ValueRO.Position;
                    view.Transform.localRotation = transform.ValueRO.Rotation;
                }
            }
        }
    }
}
