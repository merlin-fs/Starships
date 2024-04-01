using System.Collections.Generic;

using Common.Core;

using UnityEngine;

namespace Common.UI.Windows
{
	public interface IWindowExitHandlerPool
	{
		void Push(IWindowExitHandler window);
		void Pop(IWindowExitHandler window);
	}

	public class WindowExitHandlerPool : IWindowExitHandlerPool
	{
		private static IWindowManager WindowManager => Inject<IWindowManager>.Value;
		List<IWindowExitHandler> m_WidowStack = new List<IWindowExitHandler>();

		protected virtual void OnTryTerminate() { }

		public void Update()
		{
			if (!Input.GetKeyDown(KeyCode.Escape))
				return;

			if (m_WidowStack.Count > 0)
				m_WidowStack[m_WidowStack.Count - 1].SendClose();
			else
			{
				if (WindowManager.IsBusy())
					return;

				OnTryTerminate();
			}
		}

		void IWindowExitHandlerPool.Push(IWindowExitHandler window)
		{ 
			m_WidowStack.Add(window); 
		}

		void IWindowExitHandlerPool.Pop(IWindowExitHandler window)
		{ 
			m_WidowStack.Remove(window); 
		}
	}
}