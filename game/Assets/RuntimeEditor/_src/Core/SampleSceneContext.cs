using System;
using Common.Core;

using Game.Core.Repositories;

namespace Game
{
    public class SampleSceneContext : DIContext
    {
        protected override void OnBind()
        {
            Bind<ObjectRepository>(new ObjectRepository());
            Bind<AnimationRepository>(new AnimationRepository());
        }
    }
}