using System.Text;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class EncodingHelperTests
    {
        [Fact]
        public void GetString_UTF8_Normal()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var text = "Hello, 世界!";
            var bytes = encoding.GetBytes(text);

            // Act
            var result = encoding.GetString(bytes);

            // Assert
            Assert.Equal(text, result);
        }

        [Fact]
        public void GetString_Empty_ReturnsEmpty()
        {
            var encoding = Encoding.UTF8;
            var bytes = Array.Empty<byte>();
            var result = encoding.GetString(bytes);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetString_ASCII_Works()
        {
            var encoding = Encoding.ASCII;
            var text = "ABC123";
            var bytes = encoding.GetBytes(text);
            var result = encoding.GetString(bytes);

            Assert.Equal(text, result);
        }

        [Fact]
        public void GetString_Unicode_Works()
        {
            var encoding = Encoding.Unicode;
            var text = "测试123";
            var bytes = encoding.GetBytes(text);
            var result = encoding.GetString(bytes);

            Assert.Equal(text, result);
        }

        [Fact]
        public void GetBytes_UTF8_Normal()
        {
            var encoding = Encoding.UTF8;
            var text = "Hello";
            var expected = encoding.GetBytes(text);
            Span<byte> buffer = stackalloc byte[expected.Length];

            var count = encoding.GetBytes(text.AsSpan(), buffer);

            Assert.Equal(count, expected.Length);
            Assert.Equal(expected, buffer.ToArray());
        }

        [Fact]
        public void GetBytes_Unicode_Works()
        {
            var encoding = Encoding.Unicode;
            var text = "你好";
            var expected = encoding.GetBytes(text);
            Span<byte> buffer = stackalloc byte[expected.Length];

            var count = encoding.GetBytes(text.AsSpan(), buffer);

            Assert.Equal(count, expected.Length);
            Assert.Equal(expected, buffer.ToArray());
        }

        [Fact]
        public void GetBytes_And_GetString_AreInverse()
        {
            var encoding = Encoding.UTF8;
            var text = "🌍🚀 你好 C#";

            Span<byte> buffer = stackalloc byte[encoding.GetByteCount(text)];
            var written = encoding.GetBytes(text.AsSpan(), buffer);
            var decoded = encoding.GetString(buffer[..written]);

            Assert.Equal(text, decoded);
        }
    }
}
