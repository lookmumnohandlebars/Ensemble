using System.Threading.Tasks.Dataflow;

namespace Ensemble.Coroutines.Models.Async;

public class AsyncCoroutine<TProduced> : ICoroutineWithoutOutput
{
    public AsyncCoroutine(IAsyncEnumerable<TProduced> producer, Func<TProduced, CancellationToken, Task> consumer, int dop)
    {
        _producer = producer;
        _consumer = consumer;
        _dop = dop;
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
        await foreach (var produced in _producer.WithCancellation(cancellationToken))
        {
            actionBlock.Post(produced);
        }
        actionBlock.Complete();
        await actionBlock.Completion.WaitAsync(cancellationToken);
        return Unit.Value;
    }
}