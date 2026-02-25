using ThingsGateway.Foundation.Common.Extension;
using Xunit;
#pragma warning disable CA1861 // 不要将常量数组作为参数

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ArrayHelperMoreTests
    {
        [Fact]
        public void ToHex_ReadOnlySpan_ShouldReturnUppercaseHex()
        {
            byte[] data = { 0x1A, 0x2B };
            var hex = ((ReadOnlySpan<byte>)data).ToHex();
            Assert.Equal("1A2B", hex);
        }

        [Fact]
        public void ToHex_WithSeparatorAndGroup_ShouldInsertSeparator()
        {
            byte[] data = { 0x1A, 0x2B, 0x3C };
            var hex = ((ReadOnlySpan<byte>)data).ToHex("-", 0);
            Assert.Equal("1A-2B-3C", hex);
        }

        [Fact]
        public void ArrayRemoveDouble_ShouldRemoveEnds()
        {
            int[] src = { 1, 2, 3, 4, 5 };
            var res = ArrayHelper.ArrayRemoveDouble(src, 1, 1);
            Assert.Equal(new[] { 2, 3, 4 }, res);
        }

        [Fact]
        public void BytesAdd_ShouldAddValueToEachByte()
        {
            byte[] src = { 1, 2, 250 };
            var res = ((ReadOnlySpan<byte>)src).BytesAdd(1);
            Assert.Equal(new byte[] { 2, 3, 251 }, res);
        }

        [Fact]
        public void ByteToByteArray_ShouldCompressBits_LSBFirst()
        {
            byte[] src = { 1, 0, 1, 0, 1, 0, 1, 1 }; // expected compressed: 0xD5
            var res = ((ReadOnlySpan<byte>)src).ByteToByteArray();
            Assert.Equal(new byte[] { 0xD5 }, res);
        }

        [Fact]
        public void BoolArrayToByte_ShouldPackBoolToByte()
        {
            bool[] src = { true, false, true, false, true, true, true, true };
            var res = ((ReadOnlySpan<bool>)src).BoolArrayToByte();
            Assert.Equal(new byte[] { 0xF5 }, res);
        }

        [Fact]
        public void BoolOnByteIndex_ShouldReturnCorrectBitAndThrowOnInvalid()
        {
            byte val = 0b_1010_0001;
            Assert.True(val.BoolOnByteIndex(0));
            Assert.False(val.BoolOnByteIndex(1));
            Assert.True(val.BoolOnByteIndex(7));

            // Replace Assert.ThrowsException with try/catch for compatibility
            try
            {
                val.BoolOnByteIndex(8);
                Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
            }
            catch (ArgumentOutOfRangeException)
            {
                // expected
            }
        }

        [Fact]
        public void GetBoolByIndex_ShouldReturnCorrectValue()
        {
            byte[] data = { 0b_0000_0010 };
            Assert.True(((ReadOnlySpan<byte>)data).GetBoolByIndex(1));
            Assert.False(((ReadOnlySpan<byte>)data).GetBoolByIndex(0));
        }

        [Fact]
        public void ByteBitsToBytes_ReadOnlySpan_ShouldExpandBitsToBytes()
        {
            // input: single byte 0b00000101 -> bits 0 and 2 set
            byte[] inBytes = { 0b_0000_0101 };
            var res = ((ReadOnlySpan<byte>)inBytes).ByteBitsToBytes(3, 0, 0xFF);
            Assert.Equal(new byte[] { 0xFF, 0x00, 0xFF }, res);
        }

        [Fact]
        public void ByteToBoolArray_ReadOnlySpan_ShouldReturnBoolArray()
        {
            byte[] inBytes = { 0b_0000_0011 };
            var res = ((ReadOnlySpan<byte>)inBytes).ByteToBoolArray(4);
            Assert.Equal(new bool[] { true, true, false, false }, res);
        }

        [Fact]
        public void SpliceArray_ShouldConcatenateArrays()
        {
            int[] a = { 1, 2 };
            int[] b = { 3 };
            var res = ArrayHelper.SpliceArray(a, b);
            Assert.Equal(new[] { 1, 2, 3 }, res);
        }

        [Fact]
        public void CombineMemoryBlocks_ShouldConcatenateMemory()
        {
            var list = new List<ReadOnlyMemory<byte>> { new byte[] { 1, 2 }, new byte[] { 3 } };
            var res = ArrayHelper.CombineMemoryBlocks(list);
            Assert.Equal(new byte[] { 1, 2, 3 }, res.ToArray());
        }

        [Fact]
        public void CopyArray_ShouldReturnDifferentReferenceWithSameContent()
        {
            int[] src = { 1, 2, 3 };
            var copy = src.CopyArray();
            Assert.Equal(src, copy);
            Assert.False(object.ReferenceEquals(src, copy));
        }

        [Fact]
        public void ToHexString_ReadOnlySpan_ShouldIncludeSegmentAndNewline()
        {
            byte[] data = { 0x1A, 0x2B, 0x3C, 0x4D };
            var hex = ((ReadOnlySpan<byte>)data).ToHexString('-', 2);
            // Expect segments between bytes and a newline after every 2 bytes
            var expected = "1A-2B" + Environment.NewLine + "3C-4D";
            Assert.Equal(expected, hex);
        }
    }
}
