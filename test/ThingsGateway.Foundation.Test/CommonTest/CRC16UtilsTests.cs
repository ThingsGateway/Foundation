using System.Buffers;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class CrcHelperTests
    {
        /// <summary>
        /// 常见Modbus CRC测试：01 03 00 00 00 0A → CRC = C5 CD（低字节在前）
        /// </summary>
        [TestMethod]
        public void Crc16Only_DefaultPolynomial_ReturnsExpected()
        {
            byte[] data = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A };

            var crc = CrcHelper.Crc16Only(data);

            CollectionAssert.AreEqual(new byte[] { 0xC5, 0xCD }, crc);
        }

        /// <summary>
        /// 测试空输入
        /// </summary>
        [TestMethod]
        public void Crc16Only_EmptyInput_ReturnsInitialValue()
        {
            byte[] data = Array.Empty<byte>();
            var crc = CrcHelper.Crc16Only(data);

            // 对空输入CRC结果是初始值经过算法迭代后的状态
            Assert.HasCount(2, crc);
        }

        /// <summary>
        /// 测试自定义多项式（如0x1021）
        /// </summary>
        [TestMethod]
        public void Crc16Only_CustomPolynomial_Works()
        {
            byte[] data = { 0x12, 0x34, 0x56, 0x78 };
            var crc = CrcHelper.Crc16Only(data, 0x1021);

            Assert.HasCount(2, crc);
        }

        /// <summary>
        /// ReadOnlySequence单段数据验证
        /// </summary>
        [TestMethod]
        public void Crc16Only_Sequence_SingleSegment_Works()
        {
            byte[] data = { 0xAA, 0xBB, 0xCC };
            ReadOnlySequence<byte> seq = new(data);

            var crc = CrcHelper.Crc16Only(seq);

            Assert.HasCount(2, crc);
        }

        /// <summary>
        /// ReadOnlySequence多段验证
        /// </summary>
        [TestMethod]
        public void Crc16Only_Sequence_MultiSegment_Works()
        {
            byte[] part1 = { 0x01, 0x02 };
            byte[] part2 = { 0x03, 0x04 };
            byte[] part3 = { 0x05, 0x06 };

            var segment3 = new BufferSegment(part3, null);
            var segment2 = new BufferSegment(part2, segment3);
            var segment1 = new BufferSegment(part1, segment2);

            ReadOnlySequence<byte> seq = new(segment1, 0, segment3, part3.Length);

            var crc = CrcHelper.Crc16Only(seq, 0xA001);

            Assert.HasCount(2, crc);
        }

        /// <summary>
        /// 辅助类：构建多段ReadOnlySequence
        /// </summary>
        private sealed class BufferSegment : ReadOnlySequenceSegment<byte>
        {
            public BufferSegment(ReadOnlyMemory<byte> memory, BufferSegment? next)
            {
                Memory = memory;
                if (next != null)
                {
                    Next = next;
                    RunningIndex = next.RunningIndex - memory.Length;
                }
            }
        }
    }
}
