using System.Reflection;

namespace Common.Core
{
    public struct Inject<T>
    {
        public static T Value { get; private set; }

        internal static void Initialize(IDiContext context)
        {
            Value = context.Get<T>();
        }
    }
    
    public partial class DiContext
    {
        private delegate void InitMethod(IDiContext context);
        private void InitVars()
        {
            var type = typeof(Inject<>);
            foreach (var iter in m_Container.Instances)
            {
                var target = type.MakeGenericType(iter);
                var dlg = target.GetDelegate<InitMethod>(nameof(Inject<int>.Initialize), BindingFlags.NonPublic | BindingFlags.Static);
                dlg?.Invoke(this);
            }
        }
    }
}