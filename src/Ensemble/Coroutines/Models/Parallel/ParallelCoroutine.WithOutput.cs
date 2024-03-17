using System.Collections.Concurrent;

namespace Ensemble;

public class ParallelCoroutine<TProduced, TOut> : ICoroutine<IEnumerable<TOut>>
{
    public ParallelCoroutine(
        IAsyncEnumerable<TProduced> producer,
        Func<TProduced, CancellationToken, Task<TOut>> consumer, int dop)
    {
        _producer = producer;
        _consumer = consumer;
        _dop = dop;
    }

    private readonly IAsyncEnumerable<TProduced> _producer;
    private readonly Func<TProduced, CancellationToken, Task<TOut>> _consumer;
    private readonly int _dop;

    public async Task<IEnumerable<TOut>> Execute(CancellationToken cancellationToken = default)
    {
        var concurrentBag = new ConcurrentBag<TOut>();
        await Parallel.ForEachAsync(
            _producer,
            new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            },
            async (produced, ct) =>
            {
                concurrentBag.Add(await _consumer(produced, ct));
            }
        );
        return concurrentBag;
    }
}