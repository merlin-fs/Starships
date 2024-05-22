using System;
using Common.Defs;
using Game.Core.Events;
using Unity.Entities;

namespace Buildings
{
    public interface IApiEditor
    {
        IEventHandler Events { get; }
        TypeIndex CurrentLayer { get; }
        IPlaceHolder AddObject(IConfig config);
        bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder);
        bool Place(IPlaceHolder placeHolder);
        bool Remove(IPlaceHolder placeHolder);
        void SetLogicActive(bool value);
    }
}