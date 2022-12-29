using System;
using Unity.Entities;

using UnityEngine;

namespace Common.Defs
{
    public interface IDef
    {
    }

    public interface IDef<T>: IDef
        where T : IDefineable
    {
    }

    /*
    public interface IConfig : ScriptableObject
    {
        GameObject Prefab { get; }
    }
    */
}