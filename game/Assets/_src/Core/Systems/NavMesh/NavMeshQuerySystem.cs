using Unity.Collections;
using Unity.Entities;

using UnityEngine.Experimental.AI;

namespace Game.Model.Worlds
{
    public partial struct NavMeshLogic
    {
        public partial struct QuerySystem : ISystem
        {
            private NavMeshWorld m_World;
            //m_World = NavMeshWorld.GetDefaultWorld();
            public void OnUpdate(ref SystemState state)
            {
                //new NavMeshQuery(m_World, Allocator.Persistent);//MaxPathSize
            }

            public unsafe struct Singleton : IComponentData
            {
                //NavMeshQuery
            }
        }
    }
}