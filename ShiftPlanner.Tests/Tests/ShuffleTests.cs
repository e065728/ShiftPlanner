using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using ShiftPlanner;

namespace ShiftPlanner.Tests
{
    public class ShuffleTests
    {
        [Fact]
        public void Shuffle_DoesNotChangeElementCount()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var original = list.Count;
            ShiftGenerator.Shuffle(list);
            list.Count.Should().Be(original);
        }
    }
}
