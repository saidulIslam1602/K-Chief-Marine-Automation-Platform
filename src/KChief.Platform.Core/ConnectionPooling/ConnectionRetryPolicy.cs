using Polly;
using Polly.Retry;

namespace KChief.Platform.Core.ConnectionPooling;

/// <summary>
/// Retry policy for connection operations.
/// </summary>
public class ConnectionRetryPolicy
{
    private readonly RetryPolicy _retryPolicy;

    public ConnectionRetryPolicy(int maxRetries = 3, TimeSpan? delay = null)
    {
        var delayValue = delay ?? TimeSpan.FromSeconds(1);

        _retryPolicy = Policy
            .Handle<Exception>(ex => IsRetryableException(ex))
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(delayValue.TotalMilliseconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    // Log retry attempt
                });
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        return await _retryPolicy.ExecuteAsync(operation);
    }

    public async Task ExecuteAsync(Func<Task> operation)
    {
        await _retryPolicy.ExecuteAsync(operation);
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex is TimeoutException ||
               ex is System.Net.Sockets.SocketException ||
               ex is System.IO.IOException ||
               (ex is AggregateException aggEx && aggEx.InnerExceptions.Any(IsRetryableException));
    }
}

