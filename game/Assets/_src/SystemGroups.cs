using System;
using Unity.Entities;
using Unity.Transforms;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameSpawnSystemGroup : ComponentSystemGroup { }
    
        [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderFirst = true)]
        public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }



    //[UpdateAfter(typeof(LateSimulationSystemGroup))]
    public class GameSystemGroup: ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
        public class GameLogicInitSystemGroup : ComponentSystemGroup { }

            [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
            public class GameLogicCommandBufferSystem : EntityCommandBufferSystem { }


        [UpdateInGroup(typeof(GameSystemGroup))]
        public class GameLogicSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderLast = true)]
        public class GameLogicDoneSystemGroup : ComponentSystemGroup { }

    
    /*
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class GameDoneSystemGroup : ComponentSystemGroup { }
    

    [UpdateInGroup(typeof(GameDoneSystemGroup), OrderLast = true)]
    public class GameDoneSystemCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }
    */

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GamePresentationSystemGroup : ComponentSystemGroup { }
}
