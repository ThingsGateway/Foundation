using System.Reflection;

using ThingsGateway.Foundation.Common.Extension;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class ConvertHelperTests
    {
        #region ObjectToString

        [TestMethod]
        public void ObjectToString_ShouldConvert()
        {
            Assert.AreEqual("123", 123.ObjectToString());
            Assert.AreEqual("", ((object?)null).ObjectToString());
        }

        #endregion

        #region GetMessage & GetTrueException

        [TestMethod]
        public void GetMessage_ShouldReturnCleanMessage()
        {
            var ex = new Exception("test");
            var msg = ex.GetMessage();
            Assert.Contains("test", msg);
        }

        [TestMethod]
        public void GetTrueException_ShouldUnwrapInner()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new TargetInvocationException(inner);
            var result = ex.GetTrueException();
            Assert.AreSame(inner, result);
        }

        #endregion

        #region ToBoolean

        [TestMethod]
        public void ToBoolean_ShouldParseVariousValues()
        {
            Assert.IsTrue("true".ToBoolean());
            Assert.IsFalse("false".ToBoolean());
            Assert.IsTrue("1".ToBoolean());
            Assert.IsFalse("0".ToBoolean());
            Assert.IsFalse("".ToBoolean(false));
        }

        [TestMethod]
        public void ToBoolean_FromList_ShouldTakeFirstNonEmpty()
        {
            var list = new List<string> { "", "1" };
            Assert.IsTrue(list.ToBoolean());
        }

        [TestMethod]
        public void ToBoolean_FromInvalid_ShouldReturnDefault()
        {
            Assert.IsTrue("abc".ToBoolean(true));
        }

        #endregion

        #region ToInt

        [TestMethod]
        public void ToInt_ShouldConvertStringAndNumber()
        {
            Assert.AreEqual(123, "123".ToInt());
            Assert.AreEqual(0, "abc".ToInt());
        }

        [TestMethod]
        public void ToInt_FromDateTime_ShouldConvertToUnixSeconds()
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 10);
            Assert.AreEqual(10, dt.ToInt());
        }

        [TestMethod]
        public void ToInt_FromDateTimeOffset_ShouldConvertToUnixSeconds()
        {
            var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 5, TimeSpan.Zero);
            Assert.AreEqual(5, dto.ToInt());
        }

        [TestMethod]
        public void ToInt_FromDateTime_MaxValue_ShouldThrow()
        {
            var dt = DateTime.MaxValue;
            Assert.AreEqual(-1, dt.ToInt());
        }

        #endregion

        #region ToLong

        [TestMethod]
        public void ToLong_ShouldConvertStringAndDateTime()
        {
            Assert.AreEqual(456L, "456".ToLong());
            var dt = new DateTime(1970, 1, 1, 0, 0, 1);
            Assert.AreEqual(1000L, dt.ToLong());
        }

        [TestMethod]
        public void ToLong_Span_ShouldWork()
        {
            ReadOnlySpan<char> span = "789";
            Assert.AreEqual(789, span.ToLong());
        }

        #endregion

        #region ToDouble

        [TestMethod]
        public void ToDouble_ShouldConvertStringAndNumber()
        {
            Assert.AreEqual(1.23, "1.23".ToDouble(), 0.0001);
            Assert.AreEqual(0, "abc".ToDouble());
        }

        [TestMethod]
        public void ToDouble_FromBytes_ShouldConvert()
        {
            var bytes = BitConverter.GetBytes(1.23);
            var result = bytes.ToDouble();
            Assert.AreEqual(1.23, result, 0.0001);
        }

        #endregion

        #region ToDecimal

        [TestMethod]
        public void ToDecimal_ShouldConvertStringAndDouble()
        {
            Assert.AreEqual(12.34m, "12.34".ToDecimal());
            Assert.AreEqual(1.23m, 1.23d.ToDecimal());
        }

        #endregion

        #region ToDateTime

        [TestMethod]
        public void ToDateTime_ShouldParseString()
        {
            var dt = "2020-01-02".ToDateTime();
            Assert.AreEqual(2020, dt.Year);
        }

        [TestMethod]
        public void ToDateTime_ShouldHandleUnixSeconds()
        {
            var dt = 10.ToDateTime();
            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 10), dt);
        }

        [TestMethod]
        public void ToDateTime_ShouldHandleUnixMilliseconds()
        {
            const long s = 1;
            var dt = s.ToDateTime();
            Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 1), dt);
        }

        #endregion

        #region ToDateTimeOffset

        [TestMethod]
        public void ToDateTimeOffset_ShouldParseString()
        {
            var dto = "2021-01-01".ToDateTimeOffset();
            Assert.AreEqual(2021, dto.Year);
        }

        [TestMethod]
        public void ToDateTimeOffset_ShouldHandleUnixSeconds()
        {
            var dto = 10.ToDateTimeOffset();
            Assert.AreEqual(1970, dto.Year);
            Assert.AreEqual(10, (dto - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        #endregion

        #region ToTimeSpan

        [TestMethod]
        public void ToTimeSpan_ShouldParseString()
        {
            var ts = "01:02:03".ToTimeSpan();
            Assert.AreEqual(1, ts.Hours);
            Assert.AreEqual(2, ts.Minutes);
            Assert.AreEqual(3, ts.Seconds);
        }

        [TestMethod]
        public void ToTimeSpan_ShouldHandleNumber()
        {
            var ts = 60.ToTimeSpan();
            Assert.AreEqual(TimeSpan.FromSeconds(60), ts);
        }

        #endregion

        #region Trim DateTime / DateTimeOffset

        [TestMethod]
        public void Trim_DateTime_ShouldTrimSeconds()
        {
            var dt = new DateTime(2020, 1, 1, 12, 34, 56, 789);
            var trimmed = dt.Trim("s");
            Assert.AreEqual(56, trimmed.Second);
            Assert.AreEqual(0, trimmed.Millisecond);
        }

        [TestMethod]
        public void Trim_DateTimeOffset_ShouldTrimToMinute()
        {
            var dto = new DateTimeOffset(2020, 1, 1, 12, 34, 56, 789, TimeSpan.Zero);
            var trimmed = dto.Trim("m");
            Assert.AreEqual(34, trimmed.Minute);
            Assert.AreEqual(0, trimmed.Second);
        }

        #endregion
    }
}
