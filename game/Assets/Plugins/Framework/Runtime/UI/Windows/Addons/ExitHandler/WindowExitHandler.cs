using Common.Core;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Windows
{
	public interface IWindowExitHandler
	{
		void SendClose();
	}

	public class WindowExitHandler : MonoBehaviour, IWindowExitHandler
	{
        private static IWindowExitHandlerPool Pool => Inject<WindowExitHandlerPool>.Value;
        
		private void Awake()
		{
            Pool.Push(this);
		}

		private void OnDestroy()
		{
            Pool.Pop(this);
		}

		void IWindowExitHandler.SendClose()
		{
			if (GetComponent<Window>() != null)
				GetComponent<Window>().CloseWindow();
			else
			{
				PointerEventData pointer = new PointerEventData(EventSystem.current);
				ExecuteEvents.Execute(gameObject, pointer, ExecuteEvents.pointerClickHandler);
			}
		}
	}
}