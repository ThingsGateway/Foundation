using ThingsGateway.Foundation.Common.Extension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ArrayHelperTests
    {
        #region ====== IsNullOrEmpty ======

        [Fact]
        public void IsNullOrEmpty_ICollection_ShouldWork()
        {
            IReadOnlyCollection<int>? data = null;
            Assert.True(data.IsNullOrEmpty());
            data = new List<int>();
            Assert.True(data.IsNullOrEmpty());
            data = new List<int> { 1 };
            Assert.False(data.IsNullOrEmpty());
        }

        #endregion

        #region ====== ArrayExpandToLength (Array/Span/Memory) ======

        [Fact]
        public void ArrayExpandToLength_Array_ShouldExpandAndShrink()
        {
            int[] arr = { 1, 2 };
            var expanded = arr.ArrayExpandToLength(4);
            Assert.Equal(4, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.Equal(2, shrunk.Length);
        }

        [Fact]
        public void ArrayExpandToLength_Array_ShouldHandleNull()
        {
            int[]? arr = null;
            var result = arr.ArrayExpandToLength(3);
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void ArrayExpandToLength_Memory_ShouldExpandAndShrink()
        {
            Memory<int> mem = new int[] { 1, 2, 3 };
            var expanded = mem.ArrayExpandToLength(5);
            Assert.Equal(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.Equal(2, shrunk.Length);
        }

        [Fact]
        public void ArrayExpandToLength_ReadOnlyMemory_ShouldExpandAndShrink()
        {
            ReadOnlyMemory<int> mem = new int[] { 1, 2, 3 };
            var expanded = mem.ArrayExpandToLength(5);
            Assert.Equal(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.Equal(2, shrunk.Length);
        }

        [Fact]
        public void ArrayExpandToLength_Span_ShouldExpandAndShrink()
        {
            Span<int> span = stackalloc int[3] { 1, 2, 3 };
            var expanded = span.ArrayExpandToLength(5);
            Assert.Equal(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.Equal(2, shrunk.Length);
        }

        [Fact]
        public void ArrayExpandToLength_ReadOnlySpan_ShouldExpandAndShrink()
        {
            ReadOnlySpan<int> span = new int[] { 1, 2, 3 };
            var expanded = span.ArrayExpandToLength(5);
            Assert.Equal(5, expanded.Length);
            var shrunk = expanded.ArrayExpandToLength(2);
            Assert.Equal(2, shrunk.Length);
        }

        #endregion

        #region ====== ArrayExpandToLengthEven ======

        [Fact]
        public void ArrayExpandToLengthEven_Array_ShouldAddOneWhenOdd()
        {
            int[] arr = { 1, 2, 3 };
            var result = arr.ArrayExpandToLengthEven();
            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void ArrayExpandToLengthEven_Array_ShouldKeepEven()
        {
            int[] arr = { 1, 2, 3, 4 };
            var result = arr.ArrayExpandToLengthEven();
            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void ArrayExpandToLengthEven_Memory_ShouldWork()
        {
            Memory<int> mem = new int[] { 1, 2, 3 };
            var result = mem.ArrayExpandToLengthEven();
            Assert.Equal(4, result.Length);
        }

        [Fact]
        public void ArrayExpandToLengthEven_ReadOnlyMemory_ShouldWork()
        {
            ReadOnlyMemory<int> mem = new int[] { 1, 2, 3 };
            var result = mem.ArrayExpandToLengthEven();
            Assert.Equal(4, result.Length);
        }

        #endregion
    }
}
