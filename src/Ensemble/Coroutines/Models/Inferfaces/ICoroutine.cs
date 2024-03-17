namespace Ensemble;

public interface ICoroutine<TOut>
{
    Task<TOut> Execute(CancellationToken cancellationToken = default);
}
