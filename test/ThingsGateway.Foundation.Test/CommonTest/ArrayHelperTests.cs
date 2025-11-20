using ThingsGateway.Foundation.Common.Extension;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class ArrayHelperTests
    {
        #region ====== IsNullOrEmpty ======

        [TestMethod]
        public void IsNullOrEmpty_ICollection_ShouldWork()
        {
            IReadOnlyCollection<int>? data = null;
            Assert.IsTrue(data.IsNullOrEmpty());
            data = new List<int>();
            Assert.IsTrue(data.IsNullOrEmpty());
            data = new List<int> { 1 };
            Assert.IsFalse(data.IsNullOrEmpty());
        }

        #endregion

        #region ====== ArrayExpandToLength (Array/Span/Memory) ======

        [TestMethod]
        public void ArrayExpandToLength_Array_ShouldExpandAndShrink()
        {
            int[] arr = { 1, 2 };
            var expanded = arr.ArrayExpandToLength(4);
            Assert.HasCount(4, expanded);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.HasCount(2, shrunk);
        }

        [TestMethod]
        public void ArrayExpandToLength_Array_ShouldHandleNull()
        {
            int[]? arr = null;
            var result = arr.ArrayExpandToLength(3);
            Assert.HasCount(3, result);
        }

        [TestMethod]
        public void ArrayExpandToLength_Memory_ShouldExpandAndShrink()
        {
            Memory<int> mem = new int[] { 1, 2, 3 };
            var expanded = mem.ArrayExpandToLength(5);
            Assert.AreEqual(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.AreEqual(2, shrunk.Length);
        }

        [TestMethod]
        public void ArrayExpandToLength_ReadOnlyMemory_ShouldExpandAndShrink()
        {
            ReadOnlyMemory<int> mem = new int[] { 1, 2, 3 };
            var expanded = mem.ArrayExpandToLength(5);
            Assert.AreEqual(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.AreEqual(2, shrunk.Length);
        }

        [TestMethod]
        public void ArrayExpandToLength_Span_ShouldExpandAndShrink()
        {
            Span<int> span = stackalloc int[3] { 1, 2, 3 };
            var expanded = span.ArrayExpandToLength(5);
            Assert.AreEqual(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.AreEqual(2, shrunk.Length);
        }

        [TestMethod]
        public void ArrayExpandToLength_ReadOnlySpan_ShouldExpandAndShrink()
        {
            ReadOnlySpan<int> span = new int[] { 1, 2, 3 };
            var expanded = span.ArrayExpandToLength(5);
            Assert.AreEqual(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.AreEqual(2, shrunk.Length);
        }

        #endregion

        #region ====== ArrayExpandToLengthEven ======

        [TestMethod]
        public void ArrayExpandToLengthEven_Array_ShouldAddOneWhenOdd()
        {
            int[] arr = { 1, 2, 3 };
            var result = arr.ArrayExpandToLengthEven();
            Assert.HasCount(4, result);
        }

        [TestMethod]
        public void ArrayExpandToLengthEven_Array_ShouldKeepEven()
        {
            int[] arr = { 1, 2, 3, 4 };
            var result = arr.ArrayExpandToLengthEven();
            Assert.HasCount(4, result);
        }

        [TestMethod]
        public void ArrayExpandToLengthEven_Memory_ShouldWork()
        {
            Memory<int> mem = new int[] { 1, 2, 3 };
            var result = mem.ArrayExpandToLengthEven();
            Assert.AreEqual(4, result.Length);
        }

        [TestMethod]
        public void ArrayExpandToLengthEven_ReadOnlyMemory_ShouldWork()
        {
            ReadOnlyMemory<int> mem = new int[] { 1, 2, 3 };
            var result = mem.ArrayExpandToLengthEven();
            Assert.AreEqual(4, result.Length);
        }

        #endregion
    }
}
