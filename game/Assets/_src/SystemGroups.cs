using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameSpawnSystemGroup : ComponentSystemGroup { }
    


    public class GameSystemGroup: ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
        public class GameLogicInitSystemGroup : ComponentSystemGroup { }

            [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
            public class GameLogicCommandBufferSystem : EntityCommandBufferSystem { }


        [UpdateInGroup(typeof(GameSystemGroup))]
        public class GameLogicSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderLast = true)]
        public class GameLogicDoneSystemGroup : ComponentSystemGroup { }



    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class GameEndSystemGroup : ComponentSystemGroup { }


    ///[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    //public class GameEndSystemCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GamePresentationSystemGroup : ComponentSystemGroup { }
}
