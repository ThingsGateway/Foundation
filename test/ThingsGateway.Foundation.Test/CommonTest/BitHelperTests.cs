using ThingsGateway.Foundation.Common.BitExtension;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class BitHelperTests
    {
        #region ===== UInt16 Set / Get =====

        [TestMethod]
        public void SetBit_UInt16_ShouldSetTrueAndFalse()
        {
            UInt16 value = 0;
            value = value.SetBit(0, true);
            Assert.AreEqual<UInt16>(1, value);

            value = value.SetBit(0, false);
            Assert.AreEqual<UInt16>(0, value);
        }

        [TestMethod]
        public void SetBits_UInt16_ShouldWriteMultipleBits()
        {
            UInt16 value = 0;
            value = value.SetBits(4, 3, 0b101); // write 3 bits 101 at pos 4
            Assert.AreEqual((UInt16)(0b101 << 4), value);

            // Verify
            var bits = value.GetBits(4, 3);
            Assert.AreEqual<UInt16>(0b101, bits);
        }

        [TestMethod]
        public void SetBits_UInt16_ShouldIgnoreInvalidLengthOrPos()
        {
            UInt16 original = 0b1010;
            var same1 = original.SetBits(16, 2, 3); // position >=16 ignored
            var same2 = original.SetBits(2, 0, 3);  // length <=0 ignored
            Assert.AreEqual(original, same1);
            Assert.AreEqual(original, same2);
        }

        [TestMethod]
        public void GetBit_UInt16_ShouldReturnTrueOrFalse()
        {
            UInt16 value = 0b10000;
            Assert.IsTrue(value.GetBit(4));
            Assert.IsFalse(value.GetBit(3));
        }

        [TestMethod]
        public void GetBits_UInt16_ShouldExtractCorrectValue()
        {
            UInt16 value = 0b1101_0000;
            var bits = value.GetBits(4, 3); // expect 0b101 (5)
            Assert.AreEqual<UInt16>(0b101, bits);
        }

        [TestMethod]
        public void GetBits_UInt16_ShouldHandleOutOfRange()
        {
            UInt16 value = 0b1111_1111;
            var bits = value.GetBits(20, 3);
            Assert.AreEqual<UInt16>(0, bits);
        }

        #endregion

        #region ===== Byte Set / Get =====

        [TestMethod]
        public void SetBit_Byte_ShouldSetTrueAndFalse()
        {
            byte value = 0;
            value = value.SetBit(0, true);
            Assert.AreEqual(1, value);

            value = value.SetBit(0, false);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void SetBit_Byte_ShouldIgnoreInvalidPosition()
        {
            byte value = 0b10101010;
            var same = value.SetBit(8, true); // out of range
            Assert.AreEqual(value, same);
        }

        [TestMethod]
        public void GetBit_Byte_ShouldReturnCorrectValue()
        {
            byte value = 0b0001_0000;
            Assert.IsTrue(value.GetBit(4));
            Assert.IsFalse(value.GetBit(3));
        }

        [TestMethod]
        public void GetBit_Byte_ShouldReturnFalseWhenOutOfRange()
        {
            byte value = 0b1111_1111;
            Assert.IsFalse(value.GetBit(8)); // invalid pos
        }

        #endregion
    }
}
