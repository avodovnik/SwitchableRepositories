using SampleApp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Repositories
{
    internal class DummyRepositoryB : IRepository<string>
    {
        public void Add(string model)
        {
            throw new NotImplementedException();
        }

        public string GetModel()
        {
            return "This is a dummy content message to test the Circuit Breaker.";
        }

        public string GetModel(System.Linq.Expressions.Expression<Func<string, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
