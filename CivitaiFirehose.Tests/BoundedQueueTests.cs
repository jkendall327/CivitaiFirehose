using FluentAssertions;

namespace CivitaiFirehose.Tests;

public sealed class BoundedQueueTests
{
    [Fact]
    public void NewItemsAppearFirst()
    {
        var queue = new BoundedQueue<int>(5);
        
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        
        queue.ToList().Should().Equal(3, 2, 1);
    }
    
    [Fact]
    public void RemovesOldestItemsWhenFull()
    {
        var queue = new BoundedQueue<int>(3);
        
        queue.Enqueue(1); // [1]
        queue.Enqueue(2); // [2, 1]
        queue.Enqueue(3); // [3, 2, 1]
        queue.Enqueue(4); // [4, 3, 2]
        
        queue.ToList().Should().Equal(4, 3, 2);
        queue.ToList().Should().NotContain(1);
    }
    
    [Fact]
    public void EmptyQueueReturnsEmptyEnumeration()
    {
        var queue = new BoundedQueue<int>(5);
        
        queue.Should().BeEmpty();
    }
    
    [Fact]
    public void CanEnumerateMultipleTimes()
    {
        var queue = new BoundedQueue<int>(5);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        
        var firstEnum = queue.ToList();
        var secondEnum = queue.ToList();
        
        firstEnum.Should().Equal(secondEnum);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void WorksWithDifferentCapacities(int capacity)
    {
        var queue = new BoundedQueue<int>(capacity);
        
        for (var i = 0; i < capacity + 1; i++)
        {
            queue.Enqueue(i);
        }
        
        queue.Count().Should().Be(capacity);
    }
}