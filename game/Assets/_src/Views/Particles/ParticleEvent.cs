using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Game.Views
{

    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleEvent : MonoBehaviour
    {
        private ParticleSystem m_Particle;

        public event Action<ParticleSystem> OnStop;

        private void Awake()
        {
            m_Particle = GetComponent<ParticleSystem>();
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