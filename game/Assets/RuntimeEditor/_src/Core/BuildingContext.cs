using System;
using Common.Core;
using UnityEngine;

namespace Buildings
{
    public class BuildingContext : DIContext
    {
        [SerializeField]
        private Config m_Config;

        protected override void OnBind()
        {
            Bind<Config>(m_Config);
        }
    }
}