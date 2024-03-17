using System.Collections.Concurrent;
using FluentAssertions;

namespace Ensemble.UnitTests;

public abstract class SimpleCoroutineWithoutOutputTests
{
    public class WithSimpleIntegerAndSpyTests
    {
        private readonly SimpleCoroutine<int> _sut;
        private readonly SpyConsumer<int> _spyConsumer;

        public WithSimpleIntegerAndSpyTests()
        {
            var producer = new List<int>() { 1, 4, 9, 16, 25 }.ToAsyncEnumerable();
            _spyConsumer = new SpyConsumer<int>();
            _sut = new SimpleCoroutine<int>(producer, _spyConsumer.Action());
        }
        
        [Fact]
        public async Task Test()
        {
            await _sut.Execute(CancellationToken.None);
            _spyConsumer.Items.Should().BeEquivalentTo(new List<int>() {1,2,3,4,5});
        }

        private class SpyConsumer<T>
        {
            public readonly List<T> Items = new();

            public Func<T, CancellationToken, Task> Action()
            {
                return (item, _) =>
                {
                    Items.Add(item);
                    return Task.CompletedTask;
                };
            }
        }
    }
}