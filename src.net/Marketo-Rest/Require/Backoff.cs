using System;
using System.Threading;
using System.Threading.Tasks;

//https://github.com/MathieuTurcotte/node-backoff/blob/master/lib/backoff.js
namespace Marketo.Require
{
    /// <summary>
    /// Abstract class defining the skeleton for the backoff strategies. Accepts an object holding the options for the backoff strategy:
    ///  * `randomisationFactor`: The randomisation factor which must be between 0 and 1 where 1 equates to a randomization factor of 100% and 0 to no randomization.
    ///  * `initialDelay`: The backoff initial delay in milliseconds.
    ///  * `maxDelay`: The backoff maximal delay in milliseconds.
    /// </summary>
    public abstract class BackoffStrategy
    {
        readonly int _initialDelay;
        readonly int _maxDelay;
        readonly int _randomisationFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackoffStrategy"/> class.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="maxDelay">The maximum delay.</param>
        /// <param name="randomisationFactor">The randomisation factor.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// options.initialDelay - The initial timeout must be greater than 0.
        /// or
        /// options.maxDelay - The maximal timeout must be greater than 0.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The maximal backoff delay must be greater than the initial backoff delay.
        /// or
        /// The randomisation factor must be between 0 and 1.
        /// </exception>
        protected BackoffStrategy(int? initialDelay, int? maxDelay, int? randomisationFactor)
        {
            if (initialDelay != null && initialDelay < 1)
                throw new ArgumentOutOfRangeException("options.initialDelay", "The initial timeout must be greater than 0.");
            else if (maxDelay != null && maxDelay < 1)
                throw new ArgumentOutOfRangeException("options.maxDelay", "The maximal timeout must be greater than 0.");
            _initialDelay = initialDelay ?? 100;
            _maxDelay = maxDelay ?? 10000;
            if (_maxDelay <= _initialDelay)
                throw new InvalidOperationException("The maximal backoff delay must be greater than the initial backoff delay.");
            if (randomisationFactor != null && (randomisationFactor < 0 || randomisationFactor > 1))
                throw new InvalidOperationException("The randomisation factor must be between 0 and 1.");
            _randomisationFactor = randomisationFactor ?? 0;
        }

        /// <summary>
        /// Gets the maximal backoff delay.
        /// </summary>
        /// <returns>System.Int32.</returns>
        protected int GetMaxDelay()
        {
            return _maxDelay;
        }

        /// <summary>
        /// Gets the initial backoff delay.
        /// </summary>
        /// <returns>System.Int32.</returns>
        protected int GetInitialDelay()
        {
            return _initialDelay;
        }

        /// <summary>
        /// Template method that computes and returns the next backoff delay in milliseconds.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int Next()
        {
            var backoffDelay = NextInternal();
            var randomisationMultiple = 1 + new Random().NextDouble() * _randomisationFactor;
            var randomizedDelay = (int)Math.Round(backoffDelay * randomisationMultiple);
            return randomizedDelay;
        }

        /// <summary>
        /// Computes and returns the next backoff delay. Intended to be overridden by subclasses.
        /// </summary>
        /// <returns>System.Double.</returns>
        protected abstract double NextInternal();

        /// <summary>
        /// Template method that resets the backoff delay to its initial value.
        /// </summary>
        public void Reset()
        {
            ResetInternal();
        }

        /// <summary>
        /// Resets the backoff delay to its initial value. Intended to be overridden by subclasses.
        /// </summary>
        public abstract void ResetInternal();
    }

    /// <summary>
    /// Exponential backoff strategy.
    /// </summary>
    /// <seealso cref="Marketo.Require.BackoffStrategy" />
    public class ExponentialBackoffStrategy : BackoffStrategy
    {
        // Default multiplication factor used to compute the next backoff delay from the current one.
        // The value can be overridden by passing a custom factor as part of the options.
        const int DEFAULT_FACTOR = 2;
        int _backoffDelay;
        int _nextBackoffDelay;
        int _factor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialBackoffStrategy"/> class.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="maxDelay">The maximum delay.</param>
        /// <param name="randomisationFactor">The randomisation factor.</param>
        /// <param name="factor">The factor.</param>
        /// <exception cref="ArgumentOutOfRangeException">options.factor</exception>
        public ExponentialBackoffStrategy(int? initialDelay = null, int? maxDelay = null, int? randomisationFactor = null, int? factor = null)
            : base(initialDelay, maxDelay, randomisationFactor)
        {
            _backoffDelay = 0;
            _nextBackoffDelay = GetInitialDelay();
            _factor = DEFAULT_FACTOR;
            if (factor != null)
            {
                if (factor <= 1) throw new ArgumentOutOfRangeException("options.factor", $"Exponential factor should be greater than 1 but got {factor}.");
                _factor = factor.Value;
            }
        }

        /// <summary>
        /// Nexts the internal.
        /// </summary>
        /// <returns>System.Double.</returns>
        protected override double NextInternal()
        {
            _backoffDelay = Math.Min(_nextBackoffDelay, GetMaxDelay());
            _nextBackoffDelay = _backoffDelay * _factor;
            return _backoffDelay;
        }

        /// <summary>
        /// Resets the internal.
        /// </summary>
        public override void ResetInternal()
        {
            _backoffDelay = 0;
            _nextBackoffDelay = GetInitialDelay();
        }
    }

    /// <summary>
    /// Fibonacci backoff strategy.
    /// </summary>
    /// <seealso cref="Marketo.Require.BackoffStrategy" />
    public class FibonacciBackoffStrategy : BackoffStrategy
    {
        int _backoffDelay;
        int _nextBackoffDelay;

        /// <summary>
        /// Initializes a new instance of the <see cref="FibonacciBackoffStrategy"/> class.
        /// </summary>
        /// <param name="initialDelay">The initial delay.</param>
        /// <param name="maxDelay">The maximum delay.</param>
        /// <param name="randomisationFactor">The randomisation factor.</param>
        public FibonacciBackoffStrategy(int? initialDelay = null, int? maxDelay = null, int? randomisationFactor = null)
            : base(initialDelay, maxDelay, randomisationFactor)
        {
            _backoffDelay = 0;
            _nextBackoffDelay = GetInitialDelay();
        }

        /// <summary>
        /// Nexts the internal.
        /// </summary>
        /// <returns>System.Double.</returns>
        protected override double NextInternal()
        {
            var backoffDelay = Math.Min(_nextBackoffDelay, GetMaxDelay());
            _nextBackoffDelay += _backoffDelay;
            _backoffDelay = backoffDelay;
            return backoffDelay;
        }

        /// <summary>
        /// Resets the internal.
        /// </summary>
        public override void ResetInternal()
        {
            _nextBackoffDelay = GetInitialDelay();
            _backoffDelay = 0;
        }
    }

    /// <summary>
    /// A class to hold the state of a backoff operation.Accepts a backoff strategy to generate the backoff delays.
    /// </summary>
    public class Backoff
    {
        BackoffStrategy _backoffStrategy;
        int _maxNumberOfRetry;
        int _backoffNumber;
        int _backoffDelay;
        CancellationTokenSource _timeoutId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Backoff"/> class.
        /// </summary>
        /// <param name="backoffStrategy">The backoff strategy.</param>
        public Backoff(BackoffStrategy backoffStrategy)
        {
            _backoffStrategy = backoffStrategy;
            _maxNumberOfRetry = -1;
            _backoffNumber = 0;
            _backoffDelay = 0;
            _timeoutId = null;
        }

        /// <summary>
        /// Gets or sets the on ready.
        /// </summary>
        /// <value>The on ready.</value>
        public Action<int, double> OnReady { get; set; }
        /// <summary>
        /// Gets or sets the on fail.
        /// </summary>
        /// <value>The on fail.</value>
        public Action<string> OnFail { get; set; }
        /// <summary>
        /// Gets or sets the on backoff.
        /// </summary>
        /// <value>The on backoff.</value>
        public Action<int, double, string> OnBackoff { get; set; }

        /// <summary>
        /// Sets a limit, greater than 0, on the maximum number of backoffs. A 'fail' event will be emitted when the limit is reached.
        /// </summary>
        /// <param name="maxNumberOfRetry">The maximum number of retry.</param>
        /// <exception cref="ArgumentOutOfRangeException">maxNumberOfRetry - Expected a maximum number of retry greater than 0 but got {maxNumberOfRetry}.</exception>
        public void FailAfter(int maxNumberOfRetry)
        {
            if (maxNumberOfRetry <= 0)
                throw new ArgumentOutOfRangeException("maxNumberOfRetry", "Expected a maximum number of retry greater than 0 but got {maxNumberOfRetry}.");
            _maxNumberOfRetry = maxNumberOfRetry;
        }

        /// <summary>
        /// Starts a backoff operation. Accepts an optional parameter to let the listeners know why the backoff operation was started.
        /// </summary>
        /// <param name="err">The error.</param>
        /// <returns>Task.</returns>
        /// <exception cref="InvalidOperationException">Backoff in progress.</exception>
        public async Task DoBackoff(string err = null)
        {
            if (_timeoutId != null)
                throw new InvalidOperationException("Backoff in progress.");
            if (_backoffNumber == _maxNumberOfRetry)
            {
                OnFail?.Invoke(err);
                Reset();
            }
            else
            {
                OnBackoff?.Invoke(_backoffNumber, _backoffDelay, err);
                _backoffDelay = _backoffStrategy.Next();
                _timeoutId = new CancellationTokenSource();
                await Task.Delay(_backoffDelay, _timeoutId.Token);
                OnBackoffInternal();
            }
        }

        /// <summary>
        /// Handles the backoff timeout completion.
        /// </summary>
        void OnBackoffInternal()
        {
            _timeoutId = null;
            OnReady?.Invoke(_backoffNumber, _backoffDelay);
            _backoffNumber++;
        }

        /// <summary>
        /// Stops any backoff operation and resets the backoff delay to its inital value.
        /// </summary>
        public void Reset()
        {
            _backoffNumber = 0;
            _backoffStrategy.Reset();
            _timeoutId?.Cancel();
            _timeoutId = null;
        }
    }
}