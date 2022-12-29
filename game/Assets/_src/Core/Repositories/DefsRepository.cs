using System;
using System.Linq;
using System.Collections.Generic;
using Common.Core;
using Common.Repositories;
using Common.Defs;

namespace Game.Core.Repositories
{
    public class DefsRepository<T> : IReadonlyRepository<ObjectID, T>
        where T : IIdentifiable<ObjectID>
    {
        public DictionaryRepository<ObjectID, T> Repo { get; } = new DictionaryRepository<ObjectID, T>();

        #region IReadonlyRepository
        IEnumerable<T> IReadonlyRepository<ObjectID, T>.Find(
            Func<T, bool> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return Repo.Find(filter, orderBy);
        }

        T IReadonlyRepository<ObjectID, T>.FindByID(ObjectID id)
        {
            return Repo.FindByID(id);
        }
        #endregion
    }
}

