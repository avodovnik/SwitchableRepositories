using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Contracts
{
    /// <summary>
    /// This is a ***VERY*** simplified repository.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface IRepository<TModel>
    {
        void Add(TModel model);
        TModel GetModel();
        TModel GetModel(Expression<Func<TModel, bool>> predicate);
    }
}
