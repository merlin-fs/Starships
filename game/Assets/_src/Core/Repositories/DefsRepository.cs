using System;
using System.Linq;
using System.Collections.Generic;
using Common.Core;
using Common.Repositories;
using Unity.Entities;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class DefsRepository<TObject> : IReadonlyRepository<ObjectID, TObject, DefsRepository<TObject>.Attribute>
        where TObject : IIdentifiable<ObjectID>
    {
        public class Attribute : IEntityAttributes<TObject>
        {
            public TObject Entity { get; }
            public HashSet<string> Labels { get; }

            public Attribute(TObject entity, string[] labels)
            {
                Entity = entity;
                Labels = labels != null 
                    ? new HashSet<string>(labels) 
                    : new HashSet<string>();
            }
        }

        protected readonly DictionaryRepository<ObjectID, TObject, Attribute> m_Repo = new DictionaryRepository<ObjectID, TObject, Attribute>();

        #region IReadonlyRepository
        public IEnumerable<TObject> Find(
            Func<Attribute, bool> filter = null,
            Func<IQueryable<Attribute>, IOrderedQueryable<Attribute>> orderBy = null)
        {
            return m_Repo.Find(filter, orderBy);
        }

        public TObject FindByID(ObjectID id)
        {
            return m_Repo.FindByID(id);
        }

        public T FindByID<T>(ObjectID id)
            where T : TObject
        {
            return m_Repo.FindByID<T>(id);
        }
        #endregion
    }
}

