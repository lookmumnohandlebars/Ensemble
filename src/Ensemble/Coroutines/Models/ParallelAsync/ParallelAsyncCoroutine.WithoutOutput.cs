using System.Threading.Tasks.Dataflow;

namespace Ensemble.Coroutines.Models.ParallelAsync;

public class ParallelAsyncCoroutine<TProduced> : ICoroutineWithoutOutput
{
    public ParallelAsyncCoroutine(
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
        var actionBlock = new ActionBlock<TProduced>(
            produced => _consumer(produced, cancellationToken),
            new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            }
        );
        await Parallel.ForEachAsync(
            _producer,
            new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            },
             (produced, _) =>
            {
                actionBlock.Post(produced);
                return ValueTask.CompletedTask;
            }
        );
        actionBlock.Complete();
        await actionBlock.Completion.WaitAsync(cancellationToken);
        return Unit.Value;
    }
}