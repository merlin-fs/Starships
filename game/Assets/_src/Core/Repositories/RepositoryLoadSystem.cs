using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Core;
using Common.Defs;
using Game.Core.Events;
using UnityEngine.AddressableAssets;

namespace Game.Core.Repositories
{
    public partial struct RepositoryLoadSystem
    {
        private static ObjectRepository ObjectRepository => Inject<ObjectRepository>.Value;
        private static AnimationRepository AnimationRepository => Inject<AnimationRepository>.Value;
        private static IEventSender Sender => Inject<IEventSender>.Value;

        public static Task<IList<IIdentifiable<ObjectID>>> LoadObjects()
        {
            Sender.SendEvent(EventRepository.GetPooled(ObjectRepository, EventRepository.Enum.Loading));
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("defs", null)
                .Task
                .ContinueWith(task =>
                {
                    ObjectRepository.Insert(task.Result.Cast<IConfig>(), "defs");
                    Sender.SendEvent(EventRepository.GetPooled(ObjectRepository, EventRepository.Enum.Done));
                    return task.Result;
                });
        }

        public static Task<IList<IIdentifiable<ObjectID>>> LoadAnimations()
        {
            Sender.SendEvent(EventRepository.GetPooled(AnimationRepository, EventRepository.Enum.Loading));
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("animation", null)
                .Task
                .ContinueWith(task =>
                {
                    AnimationRepository.Insert(task.Result, (iter) =>
                    {
                        if (iter is IInitiated initiated)
                            initiated.Initialize();
                    });
                    Sender.SendEvent(EventRepository.GetPooled(AnimationRepository, EventRepository.Enum.Done));
                    return task.Result;
                });
        }
    }
}