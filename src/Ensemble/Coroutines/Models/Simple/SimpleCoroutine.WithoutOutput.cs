using System.Collections.Concurrent;

namespace Ensemble;

/// <summary>
///     Coroutine with 1 consuming action transforming 1 produced item at a time.
/// </summary>
/// <example>
///     
/// </example>
/// <typeparam name="TProduced"></typeparam>
public class SimpleCoroutine<TProduced> : ICoroutineWithoutOutput
{
    public SimpleCoroutine(
        IAsyncEnumerable<TProduced> producer,
        Func<TProduced, CancellationToken, Task> consumer
    )
    {
        _producer = producer;
        _consumer = consumer;
    }

    private readonly IAsyncEnumerable<TProduced> _producer;
    private readonly Func<TProduced, CancellationToken, Task> _consumer;

    public async Task<Unit> Execute(CancellationToken cancellationToken = default)
    {
        await foreach (var consumable in _producer.WithCancellation(cancellationToken))
        {
            await _consumer(consumable, cancellationToken);
        }

        return Unit.Value;
    }
}