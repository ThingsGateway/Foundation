using System.Net;
using System.Text;

using ThingsGateway.Foundation.Common.Extension;
using ThingsGateway.Foundation.Common.StringExtension;
#pragma warning disable CA1861 // 不要将常量数组作为参数
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class StringHelperTests
    {
        [TestMethod]
        public void BasicEndsWithStartsContains_And_Nulls()
        {
            Assert.IsTrue("abc.txt".EndsWithIgnoreCase(".TXT", ".log"));
            Assert.IsFalse("abc.txt".EndsWithIgnoreCase(".doc"));
            Assert.IsFalse(((string?)null).EndsWithIgnoreCase(".txt"));

            Assert.IsTrue("Abc".StartsWithIgnoreCase('a'));
            Assert.IsTrue("abc".StartsWithIgnoreCase("A"));
            Assert.IsFalse("abc".StartsWithIgnoreCase('x'));
            Assert.IsFalse(((string?)null).StartsWithIgnoreCase('a'));

            Assert.IsTrue(((string?)"x").HasValue());
            Assert.IsFalse(((string?)null).HasValue());
            Assert.IsTrue(((string?)null).IsNullOrEmpty());
            Assert.IsTrue(" ".IsNullOrWhiteSpace());
        }

        [TestMethod]
        public void SplitAndJoin_Works()
        {
            var result = "1, 2;3".SplitAsInt();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);

            Assert.IsEmpty(((string?)null).SplitAsInt());
            Assert.IsEmpty("".SplitAsInt());

            var dict = "a=1;b=2;c=3".SplitAsDictionary();
            Assert.AreEqual("1", dict["a"]);
            Assert.AreEqual("2", dict["b"]);
            Assert.AreEqual("3", dict["c"]);

            dict = "x='1';y=\"2\"".SplitAsDictionary(trimQuotation: true);
            Assert.AreEqual("1", dict["x"]);
            Assert.AreEqual("2", dict["y"]);

            dict = "a".SplitAsDictionary("=", ";");
            Assert.IsTrue(dict.ContainsKey("[0]"));

            Assert.AreEqual(2, "a,b".SplitByComma()?.Length);
            Assert.AreEqual(2, "a-b".SplitByHyphen()?.Length);
            Assert.AreEqual(2, "a;b".SplitStringBySemicolon()?.Length);
            Assert.AreEqual(2, "a.b".SplitStringByDelimiter()?.Length);
            Assert.AreEqual(2, "a/b".SplitStringBySlash()?.Length);

            var list = new[] { "A", "B" };
            Assert.AreEqual("A,B", list.Join());
            Assert.AreEqual("1|2", new[] { 1, 2 }.Join("|"));

            var arr = new[] { "One", "Two" };
            Assert.IsTrue(arr.ContainsIgnoreCase("one"));
            Assert.IsFalse(arr.ContainsIgnoreCase("three"));

            Assert.IsTrue("abcDEF".ContainsIgnoreCase("def"));
            Assert.IsTrue("abcDEF".EndsWithIgnoreCase("DEF"));
            Assert.IsTrue("abcDEF".StartsWithIgnoreCase("ABC"));

            Assert.IsTrue("abc".EndsWithIgnoreCase('c'));
            Assert.IsFalse(((string?)null).EndsWithIgnoreCase('c'));
        }

        [TestMethod]
        public void Substring_TrimInvisible_Bcd_Hex()
        {
            string text = "abc<target>xyz";
            Assert.AreEqual("target", text.Substring("<", ">"));
            Assert.AreEqual("xyz", text.Substring(">", null));
            Assert.AreEqual("abc", text.Substring(null, "<"));
            Assert.AreEqual(string.Empty, text.Substring("missing", ">"));

            string s = "A\u0001B\u007FC";
            string result = s.AsSpan().TrimInvisible();
            Assert.AreEqual("ABC", result);

            var bytes = "1234".GetBytesByBCD(BcdFormatEnum.C8421);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x34 }, bytes);

            // odd length
            var bOdd = "123".GetBytesByBCD(BcdFormatEnum.C8421);
            CollectionAssert.AreEqual(new byte[] { 0x12, 0x30 }, bOdd);

            var mem = "0A1B2C".HexStringToBytes();
            CollectionAssert.AreEqual(new byte[] { 0x0A, 0x1B, 0x2C }, mem.ToArray());

            var mem1 = "0A--2C".HexStringToBytes();
            CollectionAssert.AreEqual(new byte[] { 0x0A, 0x2C }, mem1.ToArray());

            var memInvalid = "0A1-2C".HexStringToBytes();
            Assert.AreEqual(2, memInvalid.Length);

            Assert.AreEqual(0, "".HexStringToBytes().Length);
            Assert.AreEqual(0, ((string?)null).HexStringToBytes().Length);
        }

        [TestMethod]
        public void GetBytes_Encoding_Works()
        {
            var bytes = "abc".GetBytes();
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("abc"), bytes);

            var bytes2 = ((string?)null).GetBytes();
            CollectionAssert.AreEqual(Array.Empty<byte>(), bytes2);

            var u = "你好".GetBytes(Encoding.Unicode);
            CollectionAssert.AreEqual(Encoding.Unicode.GetBytes("你好"), u);
        }

        [TestMethod]
        public void EnsureStartEnsureEnd_Cut_Works()
        {
            Assert.AreEqual("prefile", ("prefile").EnsureStart("pre"));
            Assert.AreEqual("prefile", "file".EnsureStart("pre"));
            Assert.AreEqual("file.txt", "file".EnsureEnd(".txt"));
            Assert.AreEqual("file.txt", "file.txt".EnsureEnd(".txt"));

            Assert.AreEqual("ab..", "abcdef".Cut(4, ".."));
            Assert.AreEqual("data", "prefix_data".CutStart("prefix_"));
            Assert.AreEqual("data", "data_suffix".CutEnd("_suffix"));

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

        [TestMethod]
        public void ReplaceAndRegex_Works()
        {
            Assert.AreEqual("Hello Earth", "Hello WORLD".ReplaceIgnoreCase("world", "Earth"));
            Assert.AreEqual("a-c", "a_c".ReplaceIgnoreCase('_', '-'));

            var r = StringHelper.RegexReplaceIgnoreCase("abcABC", "abc", "X");
            Assert.AreEqual("XX", r);
        }

        [TestMethod]
        public void GetTypeValue_AllTypes_Works()
        {
            Assert.IsTrue(typeof(int).GetTypeValue("42", out var obj));
            Assert.AreEqual(42, (int)obj!);

            Assert.IsTrue(typeof(int).GetTypeValue("0xFF", out var hexVal));
            Assert.AreEqual(255, (int)hexVal!);

            Assert.IsTrue(typeof(int?).GetTypeValue(null, out var nullVal));
            Assert.IsNull(nullVal);

            Assert.IsTrue(typeof(bool).GetTypeValue("true", out var b));
            Assert.IsTrue((bool)b!);

            Assert.IsTrue(typeof(char).GetTypeValue("Z", out var ch));
            Assert.AreEqual('Z', (char)ch!);

            Assert.IsTrue(typeof(byte).GetTypeValue("255", out var by));
            Assert.AreEqual((byte)255, (byte)by!);

            Assert.IsTrue(typeof(sbyte).GetTypeValue("-1", out var sb));
            Assert.AreEqual((sbyte)-1, (sbyte)sb!);

            Assert.IsTrue(typeof(short).GetTypeValue("0x7F", out var sh));
            Assert.AreEqual((short)0x7F, (short)sh!);

            Assert.IsTrue(typeof(ushort).GetTypeValue("0xFF", out var ush));
            Assert.AreEqual((ushort)0xFF, (ushort)ush!);

            Assert.IsTrue(typeof(uint).GetTypeValue("0xFF", out var ui));
            Assert.AreEqual((uint)0xFF, (uint)ui!);

            Assert.IsTrue(typeof(long).GetTypeValue("0xFF", out var ln));
            Assert.AreEqual(0xFFL, (long)ln!);

            Assert.IsTrue(typeof(ulong).GetTypeValue("0xFF", out var uln));
            Assert.AreEqual(0xFFUL, (ulong)uln!);

            Assert.IsTrue(typeof(float).GetTypeValue("1.5", out var f));
            Assert.AreEqual(1.5f, (float)f!, 0.0001f);

            Assert.IsTrue(typeof(double).GetTypeValue("2.5", out var d));
            Assert.AreEqual(2.5d, (double)d!, 0.0001);

            Assert.IsTrue(typeof(decimal).GetTypeValue("3.5", out var dec));
            Assert.AreEqual(3.5m, (decimal)dec!);

            Assert.IsTrue(typeof(DateTime).GetTypeValue("2020-01-02", out var dt));
            Assert.AreEqual(new DateTime(2020, 1, 2), (DateTime)dt!);

            Assert.IsTrue(typeof(DateTimeOffset).GetTypeValue("2020-01-02+00:00", out var dto));
            Assert.IsTrue(dto is DateTimeOffset);

            Assert.IsTrue(typeof(string).GetTypeValue("hello", out var s));
            Assert.AreEqual("hello", (string)s!);

            Assert.IsTrue(typeof(IPAddress).GetTypeValue("127.0.0.1", out var ip));
            Assert.AreEqual(IPAddress.Parse("127.0.0.1"), (IPAddress)ip!);

            Assert.IsTrue(typeof(DayOfWeek).GetTypeValue("Friday", out var en));
            Assert.AreEqual(DayOfWeek.Friday, (DayOfWeek)en!);
        }

        [TestMethod]
        public void GetTypeStringValue_AllTypes_Works()
        {
            Assert.IsTrue(typeof(double).GetTypeStringValue(1.23, out var s));
            Assert.IsNotNull(s);

            Assert.IsTrue(typeof(IPAddress).GetTypeStringValue(IPAddress.Parse("127.0.0.1"), out var sip));
            Assert.AreEqual("127.0.0.1", sip);

            Assert.IsTrue(typeof(DayOfWeek).GetTypeStringValue(DayOfWeek.Monday, out var enumStr));
            Assert.AreEqual("Monday", enumStr);
        }

        [TestMethod]
        public void IsMatch_Works()
        {
            Assert.IsTrue("*".IsMatch("anything"));
            Assert.IsTrue("*test*".IsMatch("mytestvalue"));
            Assert.IsTrue("abc".IsMatch("abc"));
            Assert.IsTrue("a*e".IsMatch("apple"));
            Assert.IsFalse(((string)"pattern").IsMatch((string)null));
        }

        [TestMethod]
        public void MultiParamOverloads_Works()
        {
            Assert.IsTrue("abcde".StartsWithIgnoreCase("A", "x", "y", "z", "a"));
            Assert.IsTrue("abcde".EndsWithIgnoreCase("e", "x", "y", "z", "E"));

            Assert.IsTrue("abc".ContainsIgnoreCase("a", "b"));
            Assert.IsTrue("abc".EqualIgnoreCase("ABC", "x"));

            Assert.IsTrue("abc".ContainsIgnoreCase("a", "b", "c"));
        }
    }
}
