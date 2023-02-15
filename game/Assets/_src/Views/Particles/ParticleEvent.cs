using System;
using UnityEngine;

namespace Game.Views
{

    [RequireComponent(typeof(UnityEngine.ParticleSystem))]
    public class ParticleEvent : MonoBehaviour
    {
        private UnityEngine.ParticleSystem m_Particle;

        public event Action<UnityEngine.ParticleSystem> OnStop;

        private void Awake()
        {
            m_Particle = GetComponent<UnityEngine.ParticleSystem>();
            var main = m_Particle.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnParticleSystemStopped()
        {
            OnStop?.Invoke(m_Particle);
            OnStop = null;
        }
    }
}