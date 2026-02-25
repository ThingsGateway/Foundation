using System.Reflection;

using ThingsGateway.Foundation.Common.Extension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ConvertHelperTests
    {
        #region ObjectToString

        [Fact]
        public void ObjectToString_ShouldConvert()
        {
            Assert.Equal("123", 123.ObjectToString());
            Assert.Equal("", ((object?)null).ObjectToString());
        }

        #endregion

        #region GetMessage & GetTrueException

        [Fact]
        public void GetTrueException_ShouldUnwrapInner()
        {
            var inner = new InvalidOperationException("inner");
            var ex = new TargetInvocationException(inner);
            var result = ex.GetTrueException();
            Assert.Same(inner, result);
        }

        #endregion

        #region ToBoolean

        [Fact]
        public void ToBoolean_ShouldParseVariousValues()
        {
            Assert.True("true".ToBoolean());
            Assert.False("false".ToBoolean());
            Assert.True("1".ToBoolean());
            Assert.False("0".ToBoolean());
            Assert.False("".ToBoolean(false));
        }

        [Fact]
        public void ToBoolean_FromList_ShouldTakeFirstNonEmpty()
        {
            var list = new List<string> { "", "1" };
            Assert.True(list.ToBoolean());
        }

        [Fact]
        public void ToBoolean_FromInvalid_ShouldReturnDefault()
        {
            Assert.True("abc".ToBoolean(true));
        }

        #endregion

        #region ToInt

        [Fact]
        public void ToInt_ShouldConvertStringAndNumber()
        {
            Assert.Equal(123, "123".ToInt());
            Assert.Equal(0, "abc".ToInt());
        }

        [Fact]
        public void ToInt_FromDateTime_ShouldConvertToUnixSeconds()
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 10);
            Assert.Equal(10, dt.ToInt());
        }

        [Fact]
        public void ToInt_FromDateTimeOffset_ShouldConvertToUnixSeconds()
        {
            var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 5, TimeSpan.Zero);
            Assert.Equal(5, dto.ToInt());
        }

        [Fact]
        public void ToInt_FromDateTime_MaxValue_ShouldThrow()
        {
            var dt = DateTime.MaxValue;
            Assert.Equal(-1, dt.ToInt());
        }

        #endregion

        #region ToLong

        [Fact]
        public void ToLong_ShouldConvertStringAndDateTime()
        {
            Assert.Equal(456L, "456".ToLong());
            var dt = new DateTime(1970, 1, 1, 0, 0, 1);
            Assert.Equal(1000L, dt.ToLong());
        }

        [Fact]
        public void ToLong_Span_ShouldWork()
        {
            ReadOnlySpan<char> span = "789";
            Assert.Equal(789, span.ToLong());
        }

        #endregion

        #region ToDouble

        [Fact]
        public void ToDouble_ShouldConvertStringAndNumber()
        {
            Assert.Equal(1.23, "1.23".ToDouble(), 0.0001);
            Assert.Equal(0, "abc".ToDouble());
        }

        [Fact]
        public void ToDouble_FromBytes_ShouldConvert()
        {
            var bytes = BitConverter.GetBytes(1.23);
            var result = bytes.ToDouble();
            Assert.Equal(1.23, result, 0.0001);
        }

        #endregion

        #region ToDecimal

        [Fact]
        public void ToDecimal_ShouldConvertStringAndDouble()
        {
            Assert.Equal(12.34m, "12.34".ToDecimal());
            Assert.Equal(1.23m, 1.23d.ToDecimal());
        }

        #endregion

        #region ToDateTime

        [Fact]
        public void ToDateTime_ShouldParseString()
        {
            var dt = "2020-01-02".ToDateTime();
            Assert.Equal(2020, dt.Year);
        }

        [Fact]
        public void ToDateTime_ShouldHandleUnixSeconds()
        {
            var dt = 10.ToDateTime();
            Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 10), dt);
        }

        [Fact]
        public void ToDateTime_ShouldHandleUnixMilliseconds()
        {
            const long s = 1;
            var dt = s.ToDateTime();
            Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 1), dt);
        }

        #endregion

        #region ToDateTimeOffset

        [Fact]
        public void ToDateTimeOffset_ShouldParseString()
        {
            var dto = "2021-01-01".ToDateTimeOffset();
            Assert.Equal(2021, dto.Year);
        }

        [Fact]
        public void ToDateTimeOffset_ShouldHandleUnixSeconds()
        {
            var dto = 10.ToDateTimeOffset();
            Assert.Equal(1970, dto.Year);
            Assert.Equal(10, (dto - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        #endregion

        #region ToTimeSpan

        [Fact]
        public void ToTimeSpan_ShouldParseString()
        {
            var ts = "01:02:03".ToTimeSpan();
            Assert.Equal(1, ts.Hours);
            Assert.Equal(2, ts.Minutes);
            Assert.Equal(3, ts.Seconds);
        }

        [Fact]
        public void ToTimeSpan_ShouldHandleNumber()
        {
            var ts = 60.ToTimeSpan();
            Assert.Equal(TimeSpan.FromSeconds(60), ts);
        }

        #endregion

        #region Trim DateTime / DateTimeOffset

        [Fact]
        public void Trim_DateTime_ShouldTrimSeconds()
        {
            var dt = new DateTime(2020, 1, 1, 12, 34, 56, 789);
            var trimmed = dt.Trim("s");
            Assert.Equal(56, trimmed.Second);
            Assert.Equal(0, trimmed.Millisecond);
        }

        [Fact]
        public void Trim_DateTimeOffset_ShouldTrimToMinute()
        {
            var dto = new DateTimeOffset(2020, 1, 1, 12, 34, 56, 789, TimeSpan.Zero);
            var trimmed = dto.Trim("m");
            Assert.Equal(34, trimmed.Minute);
            Assert.Equal(0, trimmed.Second);
        }

        #endregion
    }
}
