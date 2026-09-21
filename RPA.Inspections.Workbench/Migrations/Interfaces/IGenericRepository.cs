using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace RPA.Inspections.Workbench.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate);

        T First(Expression<Func<T, bool>> predicate);

        T FirstOrDefault(Expression<Func<T, bool>> predicate);

        IEnumerable<T> GetAll();

        T GetById(object obj);

        void Create(T obj);

        void Update(T obj);

        void Delete(object id);

        void Delete(T obj);
    }
}