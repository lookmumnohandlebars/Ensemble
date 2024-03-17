using System.Threading.Tasks.Dataflow;

namespace Ensemble.Coroutines.Models.ParallelAsync;

public class ParallelAsyncCoroutine<TProduced, TOut> : ICoroutine<IEnumerable<TOut>>
{
    public ParallelAsyncCoroutine(IAsyncEnumerable<TProduced> producer, Func<TProduced, CancellationToken, Task<TOut>> consumer, int dop)
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
        var transformBlock = new TransformBlock<TProduced, TOut>(
            produced => _consumer(produced, cancellationToken),
            new ExecutionDataflowBlockOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            }
        );

        var bufferBlock = new BufferBlock<TOut>();
        transformBlock.LinkTo(bufferBlock);
        await Parallel.ForEachAsync(
            _producer,
            new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = _dop
            },
            async (produced, ct) =>
            {
                transformBlock.Post(produced);
            }
        );
        transformBlock.Complete();
        await transformBlock.Completion.WaitAsync(cancellationToken);
        bufferBlock.Complete();
        bufferBlock.TryReceiveAll(out var output);
        return output!;
    }
}