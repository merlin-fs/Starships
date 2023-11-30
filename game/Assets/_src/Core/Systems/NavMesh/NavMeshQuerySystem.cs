using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

using UnityEngine.Experimental.AI;

namespace Game.Model.Worlds
{
    public partial struct NavMeshLogic
    {
        public struct QuerySystem //: ISystem
        {

            public unsafe struct Singleton : IComponentData
            {
                //NavMeshQuery
            }
        }
    }
}