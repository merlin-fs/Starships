using System.Collections;
using System.Collections.Generic;

using Common.Core;

using UnityEngine;

namespace Common.UI.Windows
{
    public class DefaultAnim : MonoBehaviour, IAdditionalBehaviour
    {
        private static IWindowManager WindowManager => Inject<IWindowManager>.Value;
        public void Play(IWindow.AnimationMode mode, float time)
        {
            switch (mode)
            {
                case IWindow.AnimationMode.Hide:
                case IWindow.AnimationMode.Close:
                    WindowManager.SetDarkVisible(false);
                    break;
                case IWindow.AnimationMode.Open:
                case IWindow.AnimationMode.Show:
                    WindowManager.SetDarkVisible(true);
                    break;
            }
        }
    }
}
