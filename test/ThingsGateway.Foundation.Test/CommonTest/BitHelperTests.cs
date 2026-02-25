using ThingsGateway.Foundation.Common.BitExtension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class BitHelperTests
    {
        #region ===== UInt16 Set / Get =====

        [Fact]
        public void SetBit_UInt16_ShouldSetTrueAndFalse()
        {
            UInt16 value = 0;
            value = value.SetBit(0, true);
            Assert.Equal<UInt16>(1, value);

            value = value.SetBit(0, false);
            Assert.Equal<UInt16>(0, value);
        }

        [Fact]
        public void SetBits_UInt16_ShouldWriteMultipleBits()
        {
            UInt16 value = 0;
            value = value.SetBits(4, 3, 0b101); // write 3 bits 101 at pos 4
            Assert.Equal((UInt16)(0b101 << 4), value);

            // Verify
            var bits = value.GetBits(4, 3);
            Assert.Equal<UInt16>(0b101, bits);
        }

        [Fact]
        public void SetBits_UInt16_ShouldIgnoreInvalidLengthOrPos()
        {
            UInt16 original = 0b1010;
            var same1 = original.SetBits(16, 2, 3); // position >=16 ignored
            var same2 = original.SetBits(2, 0, 3);  // length <=0 ignored
            Assert.Equal(original, same1);
            Assert.Equal(original, same2);
        }

        [Fact]
        public void GetBit_UInt16_ShouldReturnTrueOrFalse()
        {
            UInt16 value = 0b10000;
            Assert.True(value.GetBit(4));
            Assert.False(value.GetBit(3));
        }

        [Fact]
        public void GetBits_UInt16_ShouldExtractCorrectValue()
        {
            UInt16 value = 0b1101_0000;
            var bits = value.GetBits(4, 3); // expect 0b101 (5)
            Assert.Equal<UInt16>(0b101, bits);
        }

        [Fact]
        public void GetBits_UInt16_ShouldHandleOutOfRange()
        {
            UInt16 value = 0b1111_1111;
            var bits = value.GetBits(20, 3);
            Assert.Equal<UInt16>(0, bits);
        }

        #endregion

        #region ===== Byte Set / Get =====

        [Fact]
        public void SetBit_Byte_ShouldSetTrueAndFalse()
        {
            byte value = 0;
            value = value.SetBit(0, true);
            Assert.Equal(1, value);

            value = value.SetBit(0, false);
            Assert.Equal(0, value);
        }

        [Fact]
        public void SetBit_Byte_ShouldIgnoreInvalidPosition()
        {
            byte value = 0b10101010;
            var same = value.SetBit(8, true); // out of range
            Assert.Equal(value, same);
        }

        [Fact]
        public void GetBit_Byte_ShouldReturnCorrectValue()
        {
            byte value = 0b0001_0000;
            Assert.True(value.GetBit(4));
            Assert.False(value.GetBit(3));
        }

        [Fact]
        public void GetBit_Byte_ShouldReturnFalseWhenOutOfRange()
        {
            byte value = 0b1111_1111;
            Assert.False(value.GetBit(8)); // invalid pos
        }

        #endregion
    }
}
