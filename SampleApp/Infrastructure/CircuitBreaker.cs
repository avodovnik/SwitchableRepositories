using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SampleApp.Infrastructure
{
    public enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpened
    }
    /// <summary>
    ///  Warning: this is intended purely for NTK demonstration purposes
    /// and should by no means be used in a production environment. Ever.
    /// 
    /// No, really. Ever.
    /// </summary>
    internal class CircuitBreaker
    {
        private readonly int _threshold;
        private readonly TimeSpan _timeout;
        private int _failureCount = 0;
        private readonly System.Timers.Timer _timer;

        public CircuitBreaker(int threshold, TimeSpan timeout)
        {
            this.State = CircuitBreakerState.Closed;

            _threshold = threshold;
            _timeout = timeout;

            _timer = new System.Timers.Timer(timeout.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("======== Circuit Breaker on probation ==========");
            // switch to Half Open
            if (this.State != CircuitBreakerState.Open) return;

            ChangeState(CircuitBreakerState.HalfOpened);

            this._timer.Stop();
        }

        public void Execute(Action action)
        {
            InternalExecute(action);
        }

        public TModel Execute<TModel>(Func<TModel> function)
        {
            var result = default(TModel);
            InternalExecute(() =>
            {
                result = function();
            });

            return result;
        }

        private void InternalExecute(Action action)
        {
            if (this.State == CircuitBreakerState.Open)
            {
                throw new OpenCircuitException("Circuit Breaker is open.");
            }

            try
            {
                //Debug.WriteLine("Executing CB action.");
                action();
                //Debug.WriteLine("CB success.");
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                {
                    Debug.WriteLine("Inner Exception null exception occurred.");
                    // called by the target of the invocation, re.throw
                    // TODO: remove this, becuase we are testing with notimplemented
                    // throw;
                }

                // TODO: we should check if the exception is blacklisted
                if (this.State == CircuitBreakerState.HalfOpened)
                {
                    // trip immediately
                    Trip();
                }
                else if (this._failureCount < _threshold)
                {
                    Interlocked.Increment(ref this._failureCount);

                    // we could raise a Service Level Changed event here, if we measured it :)
                }
                else if (this._failureCount >= this._threshold)
                {
                    Trip();
                }

                throw new OperationFailedException("Operation failed", e.InnerException);
            }

            if (this.State == CircuitBreakerState.HalfOpened)
            {
                Reset();
            }

            if (this._failureCount > 0)
            {
                // we should only decrement, if measuring SLA
                _failureCount = 0;
            }
        }

        private void Reset()
        {
            if (this.State == CircuitBreakerState.Closed) return;

            ChangeState(CircuitBreakerState.Closed);
            this._timer.Stop();

            Debug.WriteLine("CB Reset.");
        }

        private void Trip()
        {
            if (this.State == CircuitBreakerState.Open) return;

            Debug.WriteLine("Circuit Breaker Tripped");
            ChangeState(CircuitBreakerState.Open);
            this._timer.Start(); // start the timeout timer
        }

        private void ChangeState(CircuitBreakerState newState)
        {
            this.State = newState;
            Debug.WriteLine("CB State Change: {0}", newState);

            // raise event
            if (this.StateChanged != null)
            {
                StateChanged(this, null);
            }
        }

        public bool Open { get { return this.State == CircuitBreakerState.Open; } }
        public CircuitBreakerState State { get; private set; }
        public EventHandler StateChanged;
    }

    internal class OperationFailedException : Exception
    {
        public OperationFailedException(string operationFailed, Exception innerException)
            : base(operationFailed, innerException)
        {
        }
    }

    internal class OpenCircuitException : Exception
    {
        public OpenCircuitException(string circuitBreakerIsOpen)
            : base(circuitBreakerIsOpen)
        {

        }
    }
}
