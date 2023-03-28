using System;
using System.Reflection;

namespace Common.Core
{
    public partial class DIContext
    {
        public struct Var<T>
        {
            private static T m_Value;
            
            public T Value => m_Value;

            internal static void Initialize(IDIContext context)
            {
                m_Value = context.Get<T>();
            }
        }

        private delegate void InitMethod(IDIContext context);
        
        private void InitVars()
        {
            var type = typeof(Var<>);
            foreach (var iter in m_Container.Instances)
            {
                var target = type.MakeGenericType(iter);
                var method = target.GetMethod(nameof(Var<int>.Initialize), BindingFlags.NonPublic | BindingFlags.Static);
                var dlg = (InitMethod)method.CreateDelegate(typeof(InitMethod));

                dlg.Invoke(this);
            }
        }
    }
}