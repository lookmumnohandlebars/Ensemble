namespace Ensemble;

public interface INamedCoroutine<TOut> : ICoroutine<TOut>, IComparable<INamedCoroutine<TOut>>, IEquatable<INamedCoroutine<TOut>>
{
    string Name { get; }
    ICoroutine<TOut> Coroutine { get; }
}

public interface INamedCoroutineWithoutOutput : INamedCoroutine<Unit>
{ }