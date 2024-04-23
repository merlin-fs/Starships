using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Common.Core;
using Common.Core.Loading;
using Common.Defs;

using Cysharp.Threading.Tasks;

using Game.Core.Repositories;

using Reflex.Attributes;

using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Core.Loading
{
    public abstract class LoadRepositories<T> : ILoadingCommand, IProgress<float>
    {
        [SerializeField] private string label;
        
        [Inject] private ObjectRepository m_ObjectRepository;

        private float m_Progress;

        void IProgress<float>.Report(float value) => m_Progress = value;
        
        public float GetProgress()
        {
            return m_Progress; 
        }

        protected abstract AsyncOperationHandle<IList<T>> GetAsyncOperationHandle(IEnumerable keys);
        protected abstract IEnumerable<IConfig> CastToConfig(IList<T> result);
        
        public Task Execute(ILoadingManager manager)
        {
            return UniTask.Create(async () =>
            {
                await UniTask.SwitchToMainThread();
                var asyncOperationHandle = GetAsyncOperationHandle(label);
                await asyncOperationHandle
                    .ToUniTask(this)
                    .ContinueWith(async result =>
                    {
                        var configs = CastToConfig(result);
                        m_ObjectRepository.Insert(configs, label);
                        foreach (var iter in configs)
                            if (iter is IViewPrefab viewPrefab)
                            {
                                await viewPrefab.GetViewPrefab();
                            }
                    });
            }).AsTask();
        }
    }
}