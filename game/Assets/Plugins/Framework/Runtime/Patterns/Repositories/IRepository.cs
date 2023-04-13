using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Repositories
{
    public interface IEntityAttributes<T>
    {
        T Entity { get; }
    }

    public interface IReadonlyRepository<TID, TEntity, TAttr>
        where TAttr : IEntityAttributes<TEntity>
    {
        IEnumerable<TEntity> Find(Func<TAttr, bool> filter = null, Func<IQueryable<TAttr>, IOrderedQueryable<TAttr>> orderBy = null);
        TEntity FindByID(TID id);
        T FindByID<T>(TID id) where T : TEntity;
    }

    public interface IRepository<TID, TEntity, TAttr>: IReadonlyRepository<TID, TEntity, TAttr>
        where TAttr : IEntityAttributes<TEntity>
    {
        void Insert(params TAttr[] entities);
        void Insert(IEnumerable<TAttr> entities);

        void Update(params TAttr[] entities);
        void Update(IEnumerable<TAttr> entities);

        void Remove(params TAttr[] entities);
        void Remove(IEnumerable<TAttr> entities);

        TEntity[] Remove(params TID[] ids);
    }
}

