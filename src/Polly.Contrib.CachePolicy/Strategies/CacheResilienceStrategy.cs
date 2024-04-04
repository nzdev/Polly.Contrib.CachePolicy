using Polly.Contrib.CachePolicy.Utils;
using Polly.Telemetry;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Polly.Contrib.CachePolicy.Strategies
{
    public class CacheResilienceStrategy : ResilienceStrategy
    {
        private readonly ResilienceStrategyTelemetry _telemetry;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Strategy Options</param>
        /// <param name="telemetry">Telemetry</param>
        public CacheResilienceStrategy(CacheResilienceStrategyOptions options, ResilienceStrategyTelemetry telemetry)
        {
            Guard.NotNull(telemetry, nameof(telemetry));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
        }
    }
}
