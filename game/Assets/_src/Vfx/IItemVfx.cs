using Common.Core;

using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Game.Views
{
    public interface IItemVfx: IIdentifiable<Hash128>
    {
        ParticleSystem ParticleSystem { get; } 
    }
}
