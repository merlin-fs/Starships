using System;
using Common.Defs;
using Game.Core.Events;
using Unity.Entities;

namespace Buildings
{
    public interface IApiEditor
    {
        TypeIndex CurrentLayer { get; }
        IEventHandler Events { get; }
        void AddEnvironment(IConfig config);
        bool TryGetPlaceHolder(Entity entity, out IPlaceHolder holder);
    }

    public interface IApiEditorHandler
    {
        void OnSpawn(Entity entity);
        void OnPlace(Entity entity);
        void OnDestroy(Entity entity);
    }
}