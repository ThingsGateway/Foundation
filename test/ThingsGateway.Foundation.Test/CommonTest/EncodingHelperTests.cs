using System.Text;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class EncodingHelperTests
    {
        [TestMethod]
        public void GetString_UTF8_Normal()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var text = "Hello, 世界!";
            var bytes = encoding.GetBytes(text);

            // Act
            var result = encoding.GetString(bytes);

            // Assert
            Assert.AreEqual(text, result);
        }

        [TestMethod]
        public void GetString_Empty_ReturnsEmpty()
        {
            var encoding = Encoding.UTF8;
            var bytes = Array.Empty<byte>();
            var result = encoding.GetString(bytes);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetString_ASCII_Works()
        {
            var encoding = Encoding.ASCII;
            var text = "ABC123";
            var bytes = encoding.GetBytes(text);
            var result = encoding.GetString(bytes);

            Assert.AreEqual(text, result);
        }

        [TestMethod]
        public void GetString_Unicode_Works()
        {
            var encoding = Encoding.Unicode;
            var text = "测试123";
            var bytes = encoding.GetBytes(text);
            var result = encoding.GetString(bytes);

            Assert.AreEqual(text, result);
        }

        [TestMethod]
        public void GetBytes_UTF8_Normal()
        {
            var encoding = Encoding.UTF8;
            var text = "Hello";
            var expected = encoding.GetBytes(text);
            Span<byte> buffer = stackalloc byte[expected.Length];

            var count = encoding.GetBytes(text.AsSpan(), buffer);

            Assert.HasCount(count, expected);
            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void GetBytes_Unicode_Works()
        {
            var encoding = Encoding.Unicode;
            var text = "你好";
            var expected = encoding.GetBytes(text);
            Span<byte> buffer = stackalloc byte[expected.Length];

            var count = encoding.GetBytes(text.AsSpan(), buffer);

            Assert.HasCount(count, expected);
            CollectionAssert.AreEqual(expected, buffer.ToArray());
        }

        [TestMethod]
        public void GetBytes_And_GetString_AreInverse()
        {
            var encoding = Encoding.UTF8;
            var text = "🌍🚀 你好 C#";

            Span<byte> buffer = stackalloc byte[encoding.GetByteCount(text)];
            var written = encoding.GetBytes(text.AsSpan(), buffer);
            var decoded = encoding.GetString(buffer[..written]);

            Assert.AreEqual(text, decoded);
        }
    }
}
