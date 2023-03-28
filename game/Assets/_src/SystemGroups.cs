using System;
using Unity.Entities;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem
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
    public partial class GameSpawnSystemGroup : ComponentSystemGroup { }
    

    public partial class GameSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
        public partial class GameLogicInitSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameSystemGroup))]
        [UpdateAfter(typeof(GameLogicInitSystemGroup))]
        public partial class GameLogicSystemGroup : ComponentSystemGroup { }

            [UpdateInGroup(typeof(GameLogicSystemGroup), OrderLast = true)]
            public partial class GameLogicCommandBufferSystem : EntityCommandBufferSystem
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
        public partial class GameLogicEndSystemGroup : ComponentSystemGroup { }

        [UpdateInGroup(typeof(GameLogicEndSystemGroup), OrderLast = true)]
        public partial class GameLogicEndCommandBufferSystem : EntityCommandBufferSystem 
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
    public partial class GameEndSystemGroup : ComponentSystemGroup { }


    ///[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    //public class GameEndSystemCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class GamePresentationSystemGroup : ComponentSystemGroup { }
}
