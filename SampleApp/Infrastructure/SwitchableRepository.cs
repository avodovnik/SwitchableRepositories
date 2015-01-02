using Microsoft.WindowsAzure.Storage;
// originally, this sample used Azure Queues as storage, so this was
// a very "easy" choice. Naturally, it brings with some hard dependencies. 
// But, as with all the other *DEMO* code, it is not suitable for production,
// where I'm sure you are already using some retry policy implementation. Be it
// Microsoft's, or your own. 
using Microsoft.WindowsAzure.Storage.RetryPolicies;

using SampleApp.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleApp.Infrastructure
{
    // note: this is nothing more than an implementation of a repository
    public class SwitchableRepository<TModel> : IRepository<TModel>
    {
        private readonly IRepository<TModel> _underlyingRepository;
        private readonly IRepository<TModel> _backupRepository;
        private readonly CircuitBreaker _circuitBreaker;
        private readonly CircuitBreaker _backupCircuitBreaker;
        private readonly LinearRetry _retryPolicy;

        public SwitchableRepository(
            IRepository<TModel> underlyingRepository,
            IRepository<TModel> backupRepository)
        {
            _underlyingRepository = underlyingRepository;
            _backupRepository = backupRepository;

            // get the circuit breaker implementation, for our specific use
            // NOTE: we really should do this differently (e.g. distributed!)
            _circuitBreaker = new CircuitBreaker(5, TimeSpan.FromSeconds(15));
            _backupCircuitBreaker = new CircuitBreaker(1, TimeSpan.FromHours(1));

            _retryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.LinearRetry(TimeSpan.FromSeconds(1), 10);
        }

        private void RetriableWrapper(Action action)
        {
            var requestRetryPolicy = _retryPolicy.CreateInstance();
            var retryCount = 0;
            TimeSpan retryInterval = TimeSpan.FromSeconds(1);
            var opContext = new OperationContext();
            Exception previousException = null;
            do
            {
                try
                {
                    Debug.WriteLine("Executing action, count: {0}", retryCount);
                    action();
                    return;
                }
                catch (Exception e)
                {
                    retryCount++;
                    previousException = e;
                    Thread.Sleep(retryInterval);
                }
            } while (requestRetryPolicy.ShouldRetry(retryCount, 0, previousException, out retryInterval, opContext));
        }

        public void Add(TModel model)
        {
            RetriableWrapper(() => CircuitBreaker.Execute(() => Repository.Add(model)));

        }

        public TModel GetModel()
        {
            var result = default(TModel);
            RetriableWrapper(() =>
            {
                result = CircuitBreaker.Execute(() => Repository.GetModel());
            });
            return result;
        }

        public TModel GetModel(System.Linq.Expressions.Expression<Func<TModel, bool>> predicate)
        {
            var result = default(TModel);
            RetriableWrapper(() =>
            {
                result = CircuitBreaker.Execute(() => Repository.GetModel(predicate));
            });
            return result;
        }

        private IRepository<TModel> Repository
        {
            get
            {
                return _circuitBreaker.Open ? _backupRepository : _underlyingRepository;
            }
        }

        private CircuitBreaker CircuitBreaker
        {
            get { return _circuitBreaker.Open ? _backupCircuitBreaker : _circuitBreaker; }
        }
    }
}
