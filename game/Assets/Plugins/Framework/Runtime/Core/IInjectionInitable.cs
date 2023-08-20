using System;

namespace Common.Core
{
    public interface IInjectionInitable
    {
        void Init(IDiContext context);
    }
}