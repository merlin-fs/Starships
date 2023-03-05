using System;

namespace Common.Core
{
    public interface IDIContextContainer
    {
        void Bind<T>(object instance, object id = null);

        void UnBind<T>(object instance, object id = null);

        void UnBindAll();

        bool TryGet<T>(out T value, object id = null);

        bool TryGet(Type type, out object value, object id = null);
    }
}