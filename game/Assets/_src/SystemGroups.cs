using System;
using Unity.Entities;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameSpawnSystemGroup : ComponentSystemGroup { }
    


    public class GameSystemGroup: ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
        public class GameLogicInitSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup))]
        public class GameLogicSystemGroup : ComponentSystemGroup { }

            [UpdateInGroup(typeof(GameLogicSystemGroup), OrderLast = true)]
            public class GameLogicCommandBufferSystem : EntityCommandBufferSystem { }



    [UpdateInGroup(typeof(GameSystemGroup), OrderLast = true)]
        public class GameLogicEndSystemGroup : ComponentSystemGroup { }
        [UpdateInGroup(typeof(GameLogicEndSystemGroup), OrderLast = true)]
        public class GameLogicEndCommandBufferSystem : EntityCommandBufferSystem 
        {
            protected override void OnUpdate()
            {
                try
                {
                    base.OnUpdate();
                }
                catch (Exception e)
                { 
                    UnityEngine.Debug.LogException(e);
                }
            }
        }



    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class GameEndSystemGroup : ComponentSystemGroup { }


    ///[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    //public class GameEndSystemCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GamePresentationSystemGroup : ComponentSystemGroup { }
}
