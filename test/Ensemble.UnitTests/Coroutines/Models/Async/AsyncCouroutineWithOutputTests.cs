using Ensemble.Coroutines.Models.Async;

namespace Ensemble.UnitTests.Coroutines.Models.Async;

public class AsyncCouroutineWithOutputTests
{
    [Fact]
    public async Task Test()
    {
        var sut = new AsyncCoroutine<int, string>(new List<int>() { 1, 2, 3, 4, 5 }.ToAsyncEnumerable(),
            async (i, token) => i.ToString(), 5);
        var res = await sut.Execute();
        res.ToString();
    }
}