using System.Collections.Concurrent;

namespace Ensemble;

public class SimpleCoroutine<TProduced, TOut> : ICoroutine<IEnumerable<TOut>>
{
    public SimpleCoroutine(
        IAsyncEnumerable<TProduced> producer,
        Func<TProduced, CancellationToken, Task<TOut>> consumer
    )
    {
        _producer = producer;
        _consumer = consumer;
    }

    private readonly IAsyncEnumerable<TProduced> _producer;
    private readonly Func<TProduced, CancellationToken, Task<TOut>> _consumer;

    public async Task<IEnumerable<TOut>> Execute(CancellationToken cancellationToken = default)
    {
        var concurrentBag = new ConcurrentBag<TOut>();
        await foreach (var consumable in _producer.WithCancellation(cancellationToken))
        {
            concurrentBag.Add(await _consumer(consumable, cancellationToken));
        }
        return concurrentBag;
    }
}