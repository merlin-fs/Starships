using System;
using System.Linq;
using System.Collections.Generic;
using Common.Core;
using Common.Repositories;
using Unity.Entities;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class DefsRepository<Object> : IReadonlyRepository<ObjectID, Object, DefsRepository<Object>.Attribute>
        where Object : IIdentifiable<ObjectID>
    {
        public class Attribute : IEntityAttributes<Object>
        {
            public Object Entity { get; }
            public HashSet<string> Labels { get; }

            public Attribute(Object entity, string[] labels)
            {
                Entity = entity;
                Labels = new HashSet<string>(labels);
            }
        }

        protected readonly DictionaryRepository<ObjectID, Object, Attribute> m_Repo = new DictionaryRepository<ObjectID, Object, Attribute>();

        #region IReadonlyRepository
        public IEnumerable<Object> Find(
            Func<Attribute, bool> filter = null,
            Func<IQueryable<Attribute>, IOrderedQueryable<Attribute>> orderBy = null)
        {
            return m_Repo.Find(filter, orderBy);
        }

        public Object FindByID(ObjectID id)
        {
            return m_Repo.FindByID(id);
        }

        public T FindByID<T>(ObjectID id)
            where T : Object
        {
            return m_Repo.FindByID<T>(id);
        }
        #endregion
    }
}

