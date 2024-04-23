using System.Collections;
using System.Collections.Generic;

using Common.Core;

using Reflex.Attributes;

using UnityEngine;

namespace Common.UI.Windows
{
    public class DefaultAnim : MonoBehaviour, IAdditionalBehaviour
    {
        [Inject] private IWindowManager m_WindowManager;
        public void Play(IWindow.AnimationMode mode, float time)
        {
            switch (mode)
            {
                case IWindow.AnimationMode.Hide:
                case IWindow.AnimationMode.Close:
                    m_WindowManager.SetDarkVisible(false);
                    break;
                case IWindow.AnimationMode.Open:
                case IWindow.AnimationMode.Show:
                    m_WindowManager.SetDarkVisible(true);
                    break;
            }
        }
    }
}
