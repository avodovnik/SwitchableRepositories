#define FAIL  // comment this out, to get response from A everytime

using SampleApp.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Repositories
{
    internal class DummyRepositoryA : IRepository<string>
    {
        public void Add(string model)
        {
            throw new NotImplementedException();
        }

        public string GetModel()
        {
#if FAIL
            throw new NotImplementedException(); // the idea is to simulate failure
#endif
            return "Return from DummyRepositoryA";
        }

        public string GetModel(System.Linq.Expressions.Expression<Func<string, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
