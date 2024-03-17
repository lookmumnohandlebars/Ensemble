namespace Ensemble.Coroutines.Models.Named;

public class NamedCoroutine<TOut> : INamedCoroutine<TOut>
{
    public NamedCoroutine(string name, ICoroutine<TOut> coroutine)
    {
        Name = name;
        Coroutine = coroutine;
    }

    public Task<TOut> Execute(CancellationToken cancellationToken = default) => Coroutine.Execute(cancellationToken);

    public int CompareTo(INamedCoroutine<TOut>? other) =>
        other is not null ? string.Compare(other.Name, Name, StringComparison.Ordinal) : -1;

    public bool Equals(INamedCoroutine<TOut>? other) =>
        other is not null && string.Equals(other.Name, Name, StringComparison.Ordinal);

    public string Name { get; }
    public ICoroutine<TOut> Coroutine { get; }
}

public class NamedCoroutine : INamedCoroutineWithoutOutput
{
    public NamedCoroutine(string name, ICoroutineWithoutOutput coroutine)
    {
        Name = name;
        Coroutine = coroutine;
    }

    public Task<Unit> Execute(CancellationToken cancellationToken = default) => Coroutine.Execute(cancellationToken);
    
    public int CompareTo(INamedCoroutine<Unit>? other) =>
        other is not null ? string.Compare(other.Name, Name, StringComparison.Ordinal) : -1;
    public bool Equals(INamedCoroutine<Unit>? other) =>
        other is not null && string.Equals(other.Name, Name, StringComparison.Ordinal);

    public string Name { get; }
    public ICoroutine<Unit> Coroutine { get; }
}
