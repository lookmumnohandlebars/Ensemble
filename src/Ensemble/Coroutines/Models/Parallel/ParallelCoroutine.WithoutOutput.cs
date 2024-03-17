namespace Ensemble;

public class ParallelCoroutine<TProduced> : ICoroutineWithoutOutput
{
    public ParallelCoroutine(
        IAsyncEnumerable<TProduced> producer,
        Func<TProduced, CancellationToken, Task> consumer,
        int degreeOfParallelism
    )
    {
        _producer = producer;
        _consumer = consumer;
        _dop = degreeOfParallelism;
    }

    private readonly IAsyncEnumerable<TProduced> _producer;
    private readonly Func<TProduced, CancellationToken, Task> _consumer;
    private readonly int _dop;

    public async Task<Unit> Execute(CancellationToken cancellationToken = default)
    {
        await Parallel.ForEachAsync(
            _producer,
            new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            },
            async (produced, ct) =>
            {
                await _consumer(produced, ct);
            }
        );
        return Unit.Value;
    }
}
