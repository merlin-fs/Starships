using System;
using Unity.Entities;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem
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


    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameSpawnSystemGroup : ComponentSystemGroup { }
    

    public class GameSystemGroup: ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
        public class GameLogicInitSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup))]
        [UpdateAfter(typeof(GameLogicInitSystemGroup))]
        public class GameLogicSystemGroup : ComponentSystemGroup { }

            [UpdateInGroup(typeof(GameLogicSystemGroup), OrderLast = true)]
            public class GameLogicCommandBufferSystem : EntityCommandBufferSystem
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




    [UpdateInGroup(typeof(GameSystemGroup), OrderLast = true)]
        [UpdateAfter(typeof(GameLogicSystemGroup))]
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
