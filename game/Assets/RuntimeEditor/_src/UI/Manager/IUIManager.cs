using System;
using UnityEngine.UIElements;

namespace Game.UI
{
    public enum ShowStyle
    {
        Normal,
        Popup,
    }

    public interface IUIManager
    {
        void Show(VisualElement element, ShowStyle style = ShowStyle.Normal);
        void Close(VisualElement element);
        void HidePopups();
        void RestorePopups();
    }
}