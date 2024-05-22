using System;
using System.Collections.Generic;
using System.Linq;

using Common.Core;
using Common.Repositories;

namespace Game.Core.Repositories
{
    public class Repository<TObject, TID> : IReadonlyRepository<TID, TObject, Repository<TObject, TID>.Attribute>
        where TObject : IIdentifiable<TID>
    {
        protected readonly DictionaryRepository<TID, TObject, Attribute> m_Repo = new ();
        
        public class Attribute : IEntityAttributes<TObject>
        {
            public TObject Entity { get; }

            public Attribute(TObject entity)
            {
                Entity = entity;
            }
        }

        public IEnumerable<TObject> Find(Func<Attribute, bool> filter = null, Func<IQueryable<Attribute>, IOrderedQueryable<Attribute>> orderBy = null)
        {
            return m_Repo.Find(filter, orderBy);
        }

        public TObject FindByID(TID id)
        {
            return m_Repo.FindByID(id);
        }

        public T FindByID<T>(TID id) where T : TObject
        {
            return m_Repo.FindByID<T>(id);
        }

        public void Insert(TObject value) => m_Repo.Insert(value.ID, new Attribute(value));
    }
}
