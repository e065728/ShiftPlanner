using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ShiftPlanner;

namespace ShiftPlanner.Tests
{
    [TestClass]
    public class ShuffleTests
    {
        [TestMethod]
        public void Shuffle_DoesNotChangeElementCount()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var original = list.Count;
            ShiftGenerator.Shuffle(list);
            Assert.AreEqual(original, list.Count);
        }
    }
}
