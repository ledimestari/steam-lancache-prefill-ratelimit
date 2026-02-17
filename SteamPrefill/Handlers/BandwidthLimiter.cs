namespace SteamPrefill.Handlers
{
    /// <summary>
    /// A thread-safe token bucket rate limiter that constrains aggregate download bandwidth
    /// across all concurrent download tasks.
    ///
    /// Tokens represent bytes. The bucket refills based on elapsed wall-clock time.
    /// When a consumer calls ConsumeAsync, tokens are deducted. If the bucket goes negative,
    /// the consumer sleeps proportionally — but the sleep happens outside the lock so other
    /// consumers aren't blocked.
    /// </summary>
    public sealed class BandwidthLimiter
    {
        private readonly long _bytesPerSecond;
        private readonly long _maxBurstBytes;

        private long _availableTokens;
        private long _lastRefillTimestamp;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <param name="rateLimitMbps">The target rate limit in megabits per second.</param>
        public BandwidthLimiter(double rateLimitMbps)
        {
            _bytesPerSecond = (long)(rateLimitMbps * 1_000_000 / 8);
            // Allow burst up to 1 second of data
            _maxBurstBytes = _bytesPerSecond;
            _availableTokens = _maxBurstBytes;
            _lastRefillTimestamp = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Called after each ReadAsync in the download loop. If the aggregate rate would
        /// exceed the configured limit, this method will asynchronously delay the caller.
        /// </summary>
        public async Task ConsumeAsync(int bytesRead, CancellationToken cancellationToken)
        {
            double sleepSeconds;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                // Refill tokens based on elapsed time
                var currentTimestamp = Stopwatch.GetTimestamp();
                var elapsed = Stopwatch.GetElapsedTime(_lastRefillTimestamp, currentTimestamp);
                _lastRefillTimestamp = currentTimestamp;

                var tokensToAdd = (long)(elapsed.TotalSeconds * _bytesPerSecond);
                _availableTokens = Math.Min(_availableTokens + tokensToAdd, _maxBurstBytes);

                // Consume tokens
                _availableTokens -= bytesRead;

                // Calculate sleep time if overdrawn
                sleepSeconds = _availableTokens < 0
                    ? (double)-_availableTokens / _bytesPerSecond
                    : 0;
            }
            finally
            {
                _lock.Release();
            }

            // Sleep outside the lock so other tasks can continue acquiring tokens
            if (sleepSeconds > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(sleepSeconds), cancellationToken);
            }
        }
    }
}
