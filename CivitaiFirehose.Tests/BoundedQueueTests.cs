using FluentAssertions;

namespace CivitaiFirehose.Tests;

public class UnitTest1
{
    [Fact]
    public void CyclesElementsCorrectly()
    {
        var sut = new BoundedQueue<int>(5);

        var items = new[] { 1, 2, 3, 4, 5, 6 };
        
        foreach (var item in items)
        {
            sut.Enqueue(item);
        }

        var first = sut.ToList().First();

        first.Should().Be(2);
    }
}