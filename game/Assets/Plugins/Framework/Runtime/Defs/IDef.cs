using System;
using Unity.Entities;

namespace Common.Defs
{
    public interface IDef
    {
    }

    public interface IDef<T>: IDef
        where T : IDefineable
    {
    }
}