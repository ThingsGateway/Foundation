using System.Net;
using System.Text;

using ThingsGateway.Foundation.Common.Extension;
using ThingsGateway.Foundation.Common.StringExtension;
using Xunit;
#pragma warning disable CA1861 // 不要将常量数组作为参数
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

namespace ThingsGateway.Foundation.Common.Tests
{
    public class StringHelperTests
    {
        [Fact]
        public void BasicEndsWithStartsContains_And_Nulls()
        {
            Assert.True("abc.txt".EndsWithIgnoreCase(".TXT", ".log"));
            Assert.False("abc.txt".EndsWithIgnoreCase(".doc"));
            Assert.False(((string?)null).EndsWithIgnoreCase(".txt"));

            Assert.True("Abc".StartsWithIgnoreCase('a'));
            Assert.True("abc".StartsWithIgnoreCase("A"));
            Assert.False("abc".StartsWithIgnoreCase('x'));
            Assert.False(((string?)null).StartsWithIgnoreCase('a'));

            Assert.True(((string?)"x").HasValue());
            Assert.False(((string?)null).HasValue());
            Assert.True(((string?)null).IsNullOrEmpty());
            Assert.True(" ".IsNullOrWhiteSpace());
        }

        [Fact]
        public void SplitAndJoin_Works()
        {
            var result = "1, 2;3".SplitAsInt();
            Assert.Equal(new[] { 1, 2, 3 }, result);

            Assert.Empty(((string?)null).SplitAsInt());
            Assert.Empty("".SplitAsInt());

            var dict = "a=1;b=2;c=3".SplitAsDictionary();
            Assert.Equal("1", dict["a"]);
            Assert.Equal("2", dict["b"]);
            Assert.Equal("3", dict["c"]);

            dict = "x='1';y=\"2\"".SplitAsDictionary(trimQuotation: true);
            Assert.Equal("1", dict["x"]);
            Assert.Equal("2", dict["y"]);

            dict = "a".SplitAsDictionary("=", ";");
            Assert.True(dict.ContainsKey("[0]"));

            Assert.Equal(2, "a,b".SplitByComma()?.Length);
            Assert.Equal(2, "a-b".SplitByHyphen()?.Length);
            Assert.Equal(2, "a;b".SplitStringBySemicolon()?.Length);
            Assert.Equal(2, "a.b".SplitStringByDelimiter()?.Length);
            Assert.Equal(2, "a/b".SplitStringBySlash()?.Length);

            var list = new[] { "A", "B" };
            Assert.Equal("A,B", list.Join());
            Assert.Equal("1|2", new[] { 1, 2 }.Join("|"));

            var arr = new[] { "One", "Two" };
            Assert.True(arr.ContainsIgnoreCase("one"));
            Assert.False(arr.ContainsIgnoreCase("three"));

            Assert.True("abcDEF".ContainsIgnoreCase("def"));
            Assert.True("abcDEF".EndsWithIgnoreCase("DEF"));
            Assert.True("abcDEF".StartsWithIgnoreCase("ABC"));

            Assert.True("abc".EndsWithIgnoreCase('c'));
            Assert.False(((string?)null).EndsWithIgnoreCase('c'));
        }

        [Fact]
        public void Substring_TrimInvisible_Bcd_Hex()
        {
            string text = "abc<target>xyz";
            Assert.Equal("target", text.Substring("<", ">"));
            Assert.Equal("xyz", text.Substring(">", null));
            Assert.Equal("abc", text.Substring(null, "<"));
            Assert.Equal(string.Empty, text.Substring("missing", ">"));

            string s = "A\u0001B\u007FC";
            string result = s.AsSpan().TrimInvisible();
            Assert.Equal("ABC", result);

            var bytes = "1234".GetBytesByBCD(BcdFormatEnum.C8421);
            Assert.Equal(new byte[] { 0x12, 0x34 }, bytes);

            // odd length
            var bOdd = "123".GetBytesByBCD(BcdFormatEnum.C8421);
            Assert.Equal(new byte[] { 0x12, 0x30 }, bOdd);

            var mem = "0A1B2C".HexStringToBytes();
            Assert.Equal(new byte[] { 0x0A, 0x1B, 0x2C }, mem.ToArray());

            var mem1 = "0A--2C".HexStringToBytes();
            Assert.Equal(new byte[] { 0x0A, 0x2C }, mem1.ToArray());

            var memInvalid = "0A1-2C".HexStringToBytes();
            Assert.Equal(2, memInvalid.Length);

            Assert.Equal(0, "".HexStringToBytes().Length);
            Assert.Equal(0, ((string?)null).HexStringToBytes().Length);
        }

        [Fact]
        public void GetBytes_Encoding_Works()
        {
            var bytes = "abc".GetBytes();
            Assert.Equal(Encoding.UTF8.GetBytes("abc"), bytes);

            var bytes2 = ((string?)null).GetBytes();
            Assert.Equal(Array.Empty<byte>(), bytes2);

            var u = "你好".GetBytes(Encoding.Unicode);
            Assert.Equal(Encoding.Unicode.GetBytes("你好"), u);
        }

        [Fact]
        public void EnsureStartEnsureEnd_Cut_Works()
        {
            Assert.Equal("prefile", ("prefile").EnsureStart("pre"));
            Assert.Equal("prefile", "file".EnsureStart("pre"));
            Assert.Equal("file.txt", "file".EnsureEnd(".txt"));
            Assert.Equal("file.txt", "file.txt".EnsureEnd(".txt"));

            Assert.Equal("ab..", "abcdef".Cut(4, ".."));
            Assert.Equal("data", "prefix_data".CutStart("prefix_"));
            Assert.Equal("data", "data_suffix".CutEnd("_suffix"));

            // pad too long -> exception
            try
            {
                "a".Cut(1, "xx");
                Assert.Fail("Expected exception");
            }
            catch (ArgumentOutOfRangeException)
            {
                // expected
            }
        }

        [Fact]
        public void ReplaceAndRegex_Works()
        {
            Assert.Equal("Hello Earth", "Hello WORLD".ReplaceIgnoreCase("world", "Earth"));
            Assert.Equal("a-c", "a_c".ReplaceIgnoreCase('_', '-'));

            var r = StringHelper.RegexReplaceIgnoreCase("abcABC", "abc", "X");
            Assert.Equal("XX", r);
        }

        [Fact]
        public void GetTypeValue_AllTypes_Works()
        {
            Assert.True(typeof(int).GetTypeValue("42", out var obj));
            Assert.Equal(42, (int)obj!);

            Assert.True(typeof(int).GetTypeValue("0xFF", out var hexVal));
            Assert.Equal(255, (int)hexVal!);

            Assert.True(typeof(int?).GetTypeValue(null, out var nullVal));
            Assert.Null(nullVal);

            Assert.True(typeof(bool).GetTypeValue("true", out var b));
            Assert.True((bool)b!);

            Assert.True(typeof(char).GetTypeValue("Z", out var ch));
            Assert.Equal('Z', (char)ch!);

            Assert.True(typeof(byte).GetTypeValue("255", out var by));
            Assert.Equal((byte)255, (byte)by!);

            Assert.True(typeof(sbyte).GetTypeValue("-1", out var sb));
            Assert.Equal((sbyte)-1, (sbyte)sb!);

            Assert.True(typeof(short).GetTypeValue("0x7F", out var sh));
            Assert.Equal((short)0x7F, (short)sh!);

            Assert.True(typeof(ushort).GetTypeValue("0xFF", out var ush));
            Assert.Equal((ushort)0xFF, (ushort)ush!);

            Assert.True(typeof(uint).GetTypeValue("0xFF", out var ui));
            Assert.Equal((uint)0xFF, (uint)ui!);

            Assert.True(typeof(long).GetTypeValue("0xFF", out var ln));
            Assert.Equal(0xFFL, (long)ln!);

            Assert.True(typeof(ulong).GetTypeValue("0xFF", out var uln));
            Assert.Equal(0xFFUL, (ulong)uln!);

            Assert.True(typeof(float).GetTypeValue("1.5", out var f));
            Assert.Equal(1.5f, (float)f!, 0.0001f);

            Assert.True(typeof(double).GetTypeValue("2.5", out var d));
            Assert.Equal(2.5d, (double)d!, 0.0001);

            Assert.True(typeof(decimal).GetTypeValue("3.5", out var dec));
            Assert.Equal(3.5m, (decimal)dec!);

            Assert.True(typeof(DateTime).GetTypeValue("2020-01-02", out var dt));
            Assert.Equal(new DateTime(2020, 1, 2), (DateTime)dt!);

            Assert.True(typeof(DateTimeOffset).GetTypeValue("2020-01-02+00:00", out var dto));
            Assert.True(dto is DateTimeOffset);

            Assert.True(typeof(string).GetTypeValue("hello", out var s));
            Assert.Equal("hello", (string)s!);

            Assert.True(typeof(IPAddress).GetTypeValue("127.0.0.1", out var ip));
            Assert.Equal(IPAddress.Parse("127.0.0.1"), (IPAddress)ip!);

            Assert.True(typeof(DayOfWeek).GetTypeValue("Friday", out var en));
            Assert.Equal(DayOfWeek.Friday, (DayOfWeek)en!);
        }

        [Fact]
        public void GetTypeStringValue_AllTypes_Works()
        {
            Assert.True(typeof(double).GetTypeStringValue(1.23, out var s));
            Assert.NotNull(s);

            Assert.True(typeof(IPAddress).GetTypeStringValue(IPAddress.Parse("127.0.0.1"), out var sip));
            Assert.Equal("127.0.0.1", sip);

            Assert.True(typeof(DayOfWeek).GetTypeStringValue(DayOfWeek.Monday, out var enumStr));
            Assert.Equal("Monday", enumStr);
        }

        [Fact]
        public void IsMatch_Works()
        {
            Assert.True("*".IsMatch("anything"));
            Assert.True("*test*".IsMatch("mytestvalue"));
            Assert.True("abc".IsMatch("abc"));
            Assert.True("a*e".IsMatch("apple"));
            Assert.False(((string)"pattern").IsMatch((string)null));
        }

        [Fact]
        public void MultiParamOverloads_Works()
        {
            Assert.True("abcde".StartsWithIgnoreCase("A", "x", "y", "z", "a"));
            Assert.True("abcde".EndsWithIgnoreCase("e", "x", "y", "z", "E"));

            Assert.True("abc".ContainsIgnoreCase("a", "b"));
            Assert.True("abc".EqualIgnoreCase("ABC", "x"));

            Assert.True("abc".ContainsIgnoreCase("a", "b", "c"));
        }
    }
}
