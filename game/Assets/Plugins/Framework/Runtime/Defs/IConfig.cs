using System;
using Common.Core;
using Unity.Entities;

namespace Common.Defs
{
    public interface IConfig: IIdentifiable<ObjectID>
    {
        Entity Prefab { get; }
        void Configurate(Entity root, IDefineableContext context);
    }
}
