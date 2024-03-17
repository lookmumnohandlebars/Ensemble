using System.Threading.Tasks.Dataflow;

namespace Ensemble.Coroutines.Models.Async;

public class AsyncCoroutine<TProduced, TOut> : ICoroutine<IEnumerable<TOut>>
{
    public AsyncCoroutine(IAsyncEnumerable<TProduced> producer, Func<TProduced, CancellationToken, Task<TOut>> consumer, int dop)
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
        await foreach (var produced in _producer.WithCancellation(cancellationToken))
        {
            transformBlock.Post(produced);
        }
        transformBlock.Complete();
        await transformBlock.Completion.WaitAsync(cancellationToken);
        bufferBlock.Complete();
        bufferBlock.TryReceiveAll(out var output);
        return output!;
    }
}