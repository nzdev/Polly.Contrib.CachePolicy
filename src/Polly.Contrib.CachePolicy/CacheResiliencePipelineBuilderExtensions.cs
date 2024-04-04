using Polly.Contrib.CachePolicy.Strategies;
using Polly.Contrib.CachePolicy.Utils;

namespace Polly;

/// <summary>
/// Extensions for adding Cache to <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class CacheResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// A Cache policy.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The same builder instance.</returns>
    public static TBuilder AddCacheResilience<TBuilder>(this TBuilder builder, CacheResilienceStrategyOptions options)
    where TBuilder : ResiliencePipelineBuilderBase
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        builder.AddStrategy(context => new CacheResilienceStrategy(options, context.Telemetry), options);
        return builder;
    }
}