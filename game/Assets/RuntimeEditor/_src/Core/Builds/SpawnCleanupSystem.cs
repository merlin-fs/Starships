using Game;
using Game.Core.Events;
using Game.Core.Spawns;

using Reflex.Attributes;

using Unity.Entities;

namespace Buildings
{
    public class EventEditorSpawn : EventBase<EventEditorSpawn>
    {
        public Entity Entity { get; private set; }
        public int Old { get; private set; }
        
        public static EventEditorSpawn GetPooled(Entity entity, int old)
        {
            var e = EventBase<EventEditorSpawn>.GetPooled();
            e.Entity = entity;
            e.Old = old;
            return e;
        }
    }
        
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    [UpdateBefore(typeof(Spawn.CleanupSystem))]
    partial struct CleanupSystem : ISystem
    {
        [Inject] private static IEventSender m_EventSender;
        private EntityQuery m_Query;

        public void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder()
                .WithAll<Spawn.Tag, Editor.Spawn>()
                .Build();
            state.RequireForUpdate(m_Query);
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (spawn, entity) in SystemAPI.Query<Editor.Spawn>().WithEntityAccess())
            {
                m_EventSender.SendEvent(EventEditorSpawn.GetPooled(entity, spawn.Index));
            }
        }
    }
}
