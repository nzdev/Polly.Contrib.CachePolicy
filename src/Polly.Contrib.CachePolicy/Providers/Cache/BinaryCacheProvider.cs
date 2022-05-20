﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Polly.Contrib.CachePolicy.Models;
using Polly.Contrib.CachePolicy.Providers.Logging;
using Polly.Contrib.CachePolicy.Providers.Serializer;
using Polly.Contrib.CachePolicy.Utilities;

namespace Polly.Contrib.CachePolicy.Providers.Cache
{
    /// <summary>
    /// An implementation of <see cref="ICacheProvider"/> which stores cache objects in binary format.
    /// </summary>
    public class BinaryCacheProvider : ICacheProvider
    {
        /// <summary>
        /// The cache from which to get the data.
        /// </summary>
        private readonly IDistributedCache distributedCache;

        /// <summary>
        /// A serializer which convert a <see cref="CacheValue"/> to binary format representation.
        /// </summary>
        private readonly IBinarySerializer binarySerializer;

        /// <summary>
        /// Defines the contract to logging <see cref="AsyncCachePolicy{TResult}"/> metrics and traces.
        /// </summary>
        private readonly ILoggingProvider loggingProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextCacheProvider"/> class.
        /// </summary>
        /// <param name="distributedCache">The cache from which to get the data.</param>
        /// <param name="binarySerializer">A serializer which convert a <see cref="CacheValue"/> to binary format representation.</param>
        /// <param name="loggingProvider">Provides the contract to logging <see cref="AsyncCachePolicy{TResult}"/> operations</param>
        public BinaryCacheProvider(
                        IDistributedCache distributedCache,
                        IBinarySerializer binarySerializer,
                        ILoggingProvider loggingProvider)
        {
            distributedCache.ThrowIfNull(nameof(distributedCache));
            binarySerializer.ThrowIfNull(nameof(binarySerializer));
            loggingProvider.ThrowIfNull(nameof(loggingProvider));

            this.distributedCache = distributedCache;
            this.binarySerializer = binarySerializer;
            this.loggingProvider = loggingProvider;
        }

        /// <inheritdoc/>
        public async Task<TResult> GetAsync<TResult>(string key, Context context)
            where TResult : CacheValue
        {
            var stopwatch = Stopwatch.StartNew();

            var isSuccess = true;
            var isCacheHit = false;
            var isCacheFresh = false;
            Exception failureException = null;
            try
            {
                var value = await this.distributedCache.GetAsync(key);
                if (value == null)
                {
                    return null;
                }

                isCacheHit = true;
                var result = this.binarySerializer.DeserializeFromBytes<TResult>(value);
                isCacheFresh = result.IsFresh();
                return result;
            }
            catch (Exception exception)
            {
                isSuccess = false;
                failureException = exception;
                return null;
            }
            finally
            {
                this.loggingProvider.OnCacheGet(
                                                key,
                                                isSuccess,
                                                isCacheHit,
                                                isCacheFresh,
                                                stopwatch.ElapsedMilliseconds,
                                                failureException,
                                                context);
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<TResult>(string key, TResult value, TimeSpan expirationRelativeToNow, TimeSpan graceTimeRelativeToNow, Context context)
            where TResult : CacheValue
        {
            var stopwatch = Stopwatch.StartNew();

            var isSuccess = true;
            Exception failureException = null;
            try
            {
                value.SetGraceTimeStamp(graceTimeRelativeToNow);
                await this.distributedCache.SetAsync(key, this.binarySerializer.SerializeToBytes(value), new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = expirationRelativeToNow,
                });
            }
            catch (Exception exception)
            {
                isSuccess = false;
                failureException = exception;
            }
            finally
            {
                this.loggingProvider.OnCacheSet(key, isSuccess, stopwatch.ElapsedMilliseconds, failureException, context);
            }
        }
    }
}
