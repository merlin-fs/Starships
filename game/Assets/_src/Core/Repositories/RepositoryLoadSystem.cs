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
        readonly static DiContext.Var<ObjectRepository> m_ObjectRepository;
        readonly static DiContext.Var<AnimationRepository> m_AnimationRepository;
        readonly static DiContext.Var<IEventSender> m_Sender;

        public static Task<IList<IIdentifiable<ObjectID>>> LoadObjects()
        {
            m_Sender.Value.SendEvent(EventRepository.GetPooled(m_ObjectRepository.Value, EventRepository.Enum.Loading));
            var repository = m_ObjectRepository.Value; 
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("defs", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result.Cast<IConfig>(), "defs");
                    m_Sender.Value.SendEvent(EventRepository.GetPooled(m_ObjectRepository.Value, EventRepository.Enum.Done));
                    return task.Result;
                });
        }

        public static Task<IList<IIdentifiable<ObjectID>>> LoadAnimations()
        {
            m_Sender.Value.SendEvent(EventRepository.GetPooled(m_AnimationRepository.Value, EventRepository.Enum.Loading));
            var repository = m_AnimationRepository.Value; 
            return Addressables.LoadAssetsAsync<IIdentifiable<ObjectID>>("animation", null)
                .Task
                .ContinueWith(task =>
                {
                    repository.Insert(task.Result, (iter) =>
                    {
                        if (iter is IInitiated initiated)
                            initiated.Init();
                    });
                    m_Sender.Value.SendEvent(EventRepository.GetPooled(m_AnimationRepository.Value, EventRepository.Enum.Done));
                    return task.Result;
                });
        }
    }
}