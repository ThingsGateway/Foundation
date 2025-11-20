using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ThingsGateway.Foundation.Common.Extension;

public static class ConvertHelper
{
    public static readonly DateTime _dt1970 = new(1970, 1, 1);
    public static readonly DateTimeOffset _dto1970 = new(_dt1970, TimeSpan.Zero);

    public static string ObjectToString(this Object? obj)
    {
        if (obj != null) return obj is string str ? str : obj.ToString() ?? string.Empty;
        return string.Empty;
    }
    /// <summary>
    /// DateTime转Unix时间戳
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long DateTimeToUnixTimestamp(this DateTime dateTime)
    {
        // Unix 时间起点
        var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // 计算时间差
        var timeSpan = dateTime.ToUniversalTime() - unixStart;
        // 返回毫秒数
        return (long)timeSpan.TotalMilliseconds;
    }

    /// <summary>时间日期转为yyyy-MM-dd HH:mm:ss完整字符串</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="useMillisecond">是否使用毫秒</param>
    /// <param name="emptyValue">字符串空值时显示的字符串，null表示原样显示最小时间，String.Empty表示不显示</param>
    /// <returns></returns>
    public static String ToFullString(this DateTime value, Boolean useMillisecond = false, String? emptyValue = null)
    {
        if (emptyValue != null && value <= DateTime.MinValue) return emptyValue;

        var cs = useMillisecond ?
            "yyyy-MM-dd HH:mm:ss.fff".ToCharArray() :
            "yyyy-MM-dd HH:mm:ss".ToCharArray();

        var k = 0;
        var y = value.Year;
        cs[k++] = (Char)('0' + (y / 1000));
        y %= 1000;
        cs[k++] = (Char)('0' + (y / 100));
        y %= 100;
        cs[k++] = (Char)('0' + (y / 10));
        y %= 10;
        cs[k++] = (Char)('0' + y);
        k++;

        var m = value.Month;
        cs[k++] = (Char)('0' + (m / 10));
        cs[k++] = (Char)('0' + (m % 10));
        k++;

        m = value.Day;
        cs[k++] = (Char)('0' + (m / 10));
        cs[k++] = (Char)('0' + (m % 10));
        k++;

        m = value.Hour;
        cs[k++] = (Char)('0' + (m / 10));
        cs[k++] = (Char)('0' + (m % 10));
        k++;

        m = value.Minute;
        cs[k++] = (Char)('0' + (m / 10));
        cs[k++] = (Char)('0' + (m % 10));
        k++;

        m = value.Second;
        cs[k++] = (Char)('0' + (m / 10));
        cs[k++] = (Char)('0' + (m % 10));

        if (useMillisecond)
        {
            k++;
            m = value.Millisecond;
            cs[k++] = (Char)('0' + (m / 100));
            cs[k++] = (Char)('0' + (m % 100 / 10));
            cs[k++] = (Char)('0' + (m % 10));
        }

        var str = new String(cs);

        // 此格式不受其它工具识别只存不包含时区的格式
        // 取出后，业务上存的是utc取出来再当utc即可
        //if (value.Kind == DateTimeKind.Utc) str += " UTC";

        return str;
    }

    /// <summary>获取异常消息</summary>
    /// <param name="ex">异常</param>
    /// <returns></returns>
    public static String GetMessage(this Exception ex)
    {
        // 部分异常ToString可能报错，例如System.Data.SqlClient.SqlException
        try
        {

            using ValueStringBuilder valueStringBuilder = new();


            valueStringBuilder.Append(ex.Message);
            var ex1 = ex;
            for (int i = 0; i < 5; i++)
            {
                var inEx = ex1.InnerException;
                if (inEx != null)
                {
                    valueStringBuilder.Append(Environment.NewLine);
                    valueStringBuilder.Append(inEx.Message);
                    ex1 = inEx;
                }
            }

            return valueStringBuilder.ToString();
        }
        catch
        {
            return ex.Message;
        }


    }

    /// <summary>获取异常消息</summary>
    /// <param name="ex">异常</param>
    /// <returns></returns>
    public static String? GetStackTrace(this Exception ex)
    {
        // 部分异常ToString可能报错，例如System.Data.SqlClient.SqlException
        using ValueStringBuilder valueStringBuilder = new();
        try
        {



            valueStringBuilder.Append(ex.Message);
            valueStringBuilder.Append(Environment.NewLine);
            valueStringBuilder.Append(ex.StackTrace);
            var ex1 = ex;
            for (int i = 0; i < 5; i++)
            {
                var inEx = ex1.InnerException;
                if (inEx != null)
                {
                    valueStringBuilder.Append(Environment.NewLine);
                    valueStringBuilder.Append(Environment.NewLine);
                    valueStringBuilder.Append(ex.Message);
                    valueStringBuilder.Append(Environment.NewLine);
                    valueStringBuilder.Append(inEx.StackTrace);
                    ex1 = inEx;
                }
            }

            return valueStringBuilder.ToString();
        }
        catch
        {
            return ex.ToString();
        }


    }


    /// <summary>获取内部真实异常</summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static Exception GetTrueException(this Exception ex)
    {
        return ex is AggregateException agg && agg.InnerException != null
            ? GetTrueException(agg.InnerException)
            : ex is TargetInvocationException tie && tie.InnerException != null
            ? GetTrueException(tie.InnerException)
            : ex is TypeInitializationException te && te.InnerException != null
            ? GetTrueException(te.InnerException)
            : ex.GetBaseException()
            ?? ex;
    }

    /// <summary>转为布尔型。支持大小写True/False、0和非零</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Boolean ToBoolean(this Object? value, Boolean defaultValue = default)
    {
        if (value is Boolean num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 支持表单提交的StringValues
        if (value is IList<String> list)
        {
            if (list.Count == 0) return defaultValue;
            value = list.FirstOrDefault(e => !string.IsNullOrEmpty(e));
            if (value == null) return defaultValue;
        }

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (Boolean.TryParse(str, out var b)) return b;

            if (String.Equals(str, Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) return true;
            if (String.Equals(str, Boolean.FalseString, StringComparison.OrdinalIgnoreCase)) return false;

            return Int32.TryParse(str, out var n) ? n != 0 : defaultValue;
        }




        var str2 = value.ToString()?.Trim();
        if (!string.IsNullOrEmpty(str2) && Boolean.TryParse(str2, out var n2))
        {
            return n2;
        }

        if (String.Equals(str2, Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) return true;
        if (String.Equals(str2, Boolean.FalseString, StringComparison.OrdinalIgnoreCase)) return false;

        if (!string.IsNullOrEmpty(str2) && Int32.TryParse(str2, out var n3))
        {
            return n3 != 0;
        }

        try
        {
            // 转换接口
            if (value is IConvertible conv) return conv.ToBoolean(null);

            //return Convert.ToBoolean(value);
        }
        catch { }

        return defaultValue;
    }

    /// <summary>转为整数，转换失败时返回默认值。支持字符串、全角、时间（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Int32 ToInt(this Object? value, Int32 defaultValue = default)
    {
        if (value is Int32 num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            if (Int32.TryParse(str, out var n)) return n;

            // 拷贝而来的逗号分隔整数
            Span<Char> tmp = stackalloc Char[str.Length];
            var rs = TrimNumber(str.AsSpan(), tmp);
            if (rs == 0) return defaultValue;

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return Int32.TryParse(tmp[..rs], out n) ? n : defaultValue;
#else
            return Int32.TryParse(tmp[..rs].ToString(), out n) ? n : defaultValue;
#endif
        }

        // 特殊处理时间，转Unix秒
#if NET6_0_OR_GREATER
        if (value is DateOnly date) value = date.ToDateTime(TimeOnly.MinValue);
#endif
        if (value is DateTime dt)
        {
            if (dt == DateTime.MinValue) return 0;
            if (dt == DateTime.MaxValue) return -1;

            // 保存时间日期由Int32改为UInt32，原截止2038年的范围扩大到2106年
            var n = (dt - _dt1970).TotalSeconds;
            return n >= Int32.MaxValue ? throw new InvalidDataException("Time too long, value exceeds Int32.MaxValue") : (Int32)n;
        }
        if (value is DateTimeOffset dto)
        {
            if (dto == DateTimeOffset.MinValue) return 0;

            //return (Int32)(dto - _dto1970).TotalSeconds;
            var n = (dto - _dto1970).TotalSeconds;
            return n >= Int32.MaxValue ? throw new InvalidDataException("Time too long, value exceeds Int32.MaxValue") : (Int32)n;
        }


        // 转换接口
        if (value is IConvertible conv)
        {
            try
            {
                return conv.ToInt32(null);
            }
            catch { }
        }


        // 转字符串再转整数，作为兜底方案
        var str2 = value.ToString();
        return !String.IsNullOrEmpty(str2) && Int32.TryParse(str2, out var n2) ? n2 : defaultValue;
    }
    /// <summary>转为长整数。支持字符串、全角、时间（Unix毫秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Int64 ToLong(this Object? value, Int64 defaultValue = default)
    {
        if (value is Int64 num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            if (Int64.TryParse(str, out var n)) return n;

            // 拷贝而来的逗号分隔整数
            Span<Char> tmp = stackalloc Char[str.Length];
            var rs = TrimNumber(str.AsSpan(), tmp);
            if (rs == 0) return defaultValue;

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return Int64.TryParse(tmp[..rs], out n) ? n : defaultValue;
#else
            return Int64.TryParse(new String(tmp[..rs].ToArray()), out n) ? n : defaultValue;
#endif
        }

        // 特殊处理时间，转Unix毫秒
#if NET6_0_OR_GREATER
        if (value is DateOnly date) value = date.ToDateTime(TimeOnly.MinValue);
#endif
        if (value is DateTime dt)
        {
            if (dt == DateTime.MinValue) return 0;

            //// 先转UTC时间再相减，以得到绝对时间差
            //return (Int32)(dt.ToUniversalTime() - _dt1970).TotalSeconds;
            return (Int64)(dt - _dt1970).TotalMilliseconds;
        }
        if (value is DateTimeOffset dto)
        {
            return dto == DateTimeOffset.MinValue ? 0 : (Int64)(dto - _dto1970).TotalMilliseconds;
        }


        try
        {
            // 转换接口
            if (value is IConvertible conv) return conv.ToInt64(null);

            //return Convert.ToInt64(value);
        }
        catch { }

        // 转字符串再转整数，作为兜底方案
        var str2 = value.ToString();
        return !String.IsNullOrEmpty(str2) && Int64.TryParse(str2.Trim(), out var n2) ? n2 : defaultValue;
    }
    /// <summary>转为长整数。支持字符串、全角、时间（Unix毫秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Int64 ToLong(this ReadOnlySpan<char> value, Int64 defaultValue = default)
    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
        return Int64.TryParse(value, out var n) ? n : defaultValue;
#else
        return Int64.TryParse(value.ToString(), out var n) ? n : defaultValue;
#endif
    }

    /// <summary>转为浮点数</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Double ToDouble(this Object? value, Double defaultValue = default)
    {
        if (value is Double num) return Double.IsNaN(num) ? defaultValue : num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            if (Double.TryParse(str, out var n)) return n;

            Span<Char> tmp = stackalloc Char[str.Length];
            var rs = TrimNumber(str.AsSpan(), tmp);
            if (rs == 0) return defaultValue;

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return Double.TryParse(tmp[..rs], out n) ? n : defaultValue;
#else
            return Double.TryParse(new String(tmp[..rs].ToArray()), out n) ? n : defaultValue;
#endif
        }

        if (value is Byte[] buf && buf.Length <= 8) return BitConverter.ToDouble(buf, 0);

        try
        {
            // 转换接口
            if (value is IConvertible conv) return conv.ToDouble(null);

            //return Convert.ToDouble(value);
        }
        catch { }

        // 转字符串再转整数，作为兜底方案
        var str2 = value.ToString();
        return !String.IsNullOrEmpty(str2) && Double.TryParse(str2.Trim(), out var n2) ? n2 : defaultValue;
    }




    /// <summary>转为高精度浮点数</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static Decimal ToDecimal(this Object? value, Decimal defaultValue = 0)
    {
        if (value is Decimal num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            if (Decimal.TryParse(str, NumberStyles.Number | NumberStyles.AllowExponent, null, out var n)) return n;

            Span<Char> tmp = stackalloc Char[str.Length];
            var rs = TrimNumber(str.AsSpan(), tmp);
            if (rs == 0) return defaultValue;

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return Decimal.TryParse(tmp[..rs], out n) ? n : defaultValue;
#else
            return Decimal.TryParse(new String(tmp[..rs].ToArray()), out n) ? n : defaultValue;
#endif
        }

        if (value is Double d) return Double.IsNaN(d) ? defaultValue : (Decimal)d;

        try
        {
            // 转换接口
            if (value is IConvertible conv) return conv.ToDecimal(null);

            //return Convert.ToDecimal(value);
        }
        catch { }

        // 转字符串再转整数，作为兜底方案
        var str2 = value.ToString();
        return !string.IsNullOrEmpty(str2) && Decimal.TryParse(str2.Trim(), out var n2) ? n2 : defaultValue;
    }


    private static readonly Int64 _maxSeconds = (Int64)(DateTime.MaxValue - DateTime.MinValue).TotalSeconds;
    private static readonly Int64 _maxMilliseconds = (Int64)(DateTime.MaxValue - DateTime.MinValue).TotalMilliseconds;


    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(ref Utf8JsonReader reader)
    {
        // 处理时间戳自动转换
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var longValue))
        {
            return longValue.ConvertToDateTime();
        }
        var stringValue = reader.GetString();

        // 处理时间戳自动转换
        if (long.TryParse(stringValue, out var longValue2))
        {
            return longValue2.ConvertToDateTime();
        }

        return Convert.ToDateTime(stringValue);
    }



    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(ref JsonReader reader)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return JValue.ReadFrom(reader).Value<long>().ConvertToDateTime();
        }

        var stringValue = JValue.ReadFrom(reader).Value<string>();

        // 处理时间戳自动转换
        if (long.TryParse(stringValue, out var longValue2))
        {
            return longValue2.ConvertToDateTime();
        }

        return Convert.ToDateTime(stringValue);
    }


    /// <summary>
    /// 将 DateTimeOffset 转换成本地 DateTime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
            return dateTime.UtcDateTime;
        if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            return dateTime.ToLocalTime().DateTime;
        else
            return dateTime.DateTime;
    }

    /// <summary>
    /// 将 DateTimeOffset? 转换成本地 DateTime?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime? ConvertToDateTime(this DateTimeOffset? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTime() : null;
    }

    /// <summary>
    /// 将 DateTime 转换成 DateTimeOffset
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
    }

    /// <summary>
    /// 将 DateTime? 转换成 DateTimeOffset?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset? ConvertToDateTimeOffset(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTimeOffset() : null;
    }

    /// <summary>
    /// 计算2个时间差，返回文字描述
    /// </summary>
    /// <param name="beginTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>时间差</returns>
    public static string GetDiffTime(this DateTime beginTime, DateTime endTime)
    {
        TimeSpan timeDifference = endTime - beginTime;
        if (timeDifference.TotalDays >= 1)
        {
            return $"{(int)timeDifference.TotalDays} d {timeDifference.Hours} H";
        }
        else if (timeDifference.TotalHours >= 1)
        {
            return $"{(int)timeDifference.TotalHours} H {timeDifference.Minutes} m";
        }
        else
        {
            return $"{(int)timeDifference.TotalMinutes} m";
        }
    }

    /// <summary>
    /// 计算2个时间差，返回文字描述
    /// </summary>
    /// <param name="beginTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>时间差</returns>
    public static string GetDiffTime(this DateTimeOffset beginTime, DateTimeOffset endTime)
    {
        TimeSpan timeDifference = endTime - beginTime;
        if (timeDifference.TotalDays >= 1)
        {
            return $"{(int)timeDifference.TotalDays} d {timeDifference.Hours} H";
        }
        else if (timeDifference.TotalHours >= 1)
        {
            return $"{(int)timeDifference.TotalHours} H {timeDifference.Minutes} m";
        }
        else
        {
            return $"{(int)timeDifference.TotalMinutes} m";
        }
    }

    /// <summary>
    /// 返回yyyy-MM-ddTHH:mm:ss.fffffffzzz时间格式字符串
    /// </summary>
    public static string ToDefaultDateTimeFormat(this DateTime dt, TimeSpan offset)
    {
        if (dt.Kind == DateTimeKind.Utc)
            return new DateTimeOffset(dt.ToLocalTime(), offset).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        else if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        else
        {
            if (offset == TimeSpan.Zero)
            {
                return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
            }
            else if (dt.Kind != DateTimeKind.Local)
                return new DateTimeOffset(dt, offset).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }
        return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
    }

    /// <summary>
    /// 返回yyyy-MM-ddTHH:mm:ss.fffffffzzz时间格式字符串
    /// </summary>
    public static string ToDefaultDateTimeFormat(this DateTime dt)
    {
        return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
    }

    /// <summary>
    /// 返回yyyy-MM-dd HH-mm-ss-fff zz时间格式字符串
    /// </summary>
    public static string ToFileDateTimeFormat(this DateTime dt)
    {
        return ToDefaultDateTimeFormat(dt).Replace(":", "-");
    }

    /// <summary>
    /// 返回yyyy-MM-dd HH-mm-ss-fff zz时间格式字符串
    /// </summary>
    public static string ToFileDateTimeFormat(this DateTime dt, TimeSpan offset)
    {
        return ToDefaultDateTimeFormat(dt, offset).Replace(":", "-");
    }

    /// <summary>
    /// 将时间戳转换为 DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static DateTime ConvertToDateTime(this long timestamp)
    {
        var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var digitCount = (int)Math.Floor(Math.Log10(timestamp) + 1);

        if (digitCount != 13 && digitCount != 10)
        {
            throw new ArgumentException("Data is not a valid timestamp format.");
        }

        return (digitCount == 13
            ? timeStampDateTime.AddMilliseconds(timestamp)  // 13 位时间戳
            : timeStampDateTime.AddSeconds(timestamp)).ToLocalTime();   // 10 位时间戳
    }

    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    /// <remarks>
    /// 整数（Unix秒）转换后不包含时区信息，需要调用.ToLocalTime()来转换为当前时区时间
    /// </remarks>
    public static DateTime ToDateTime(this Object? value, DateTime defaultValue = default)
    {
        if (value is DateTime num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return defaultValue;

            // 处理UTC
            var utc = false;
            if (str.EndsWithIgnoreCase(" UTC"))
            {
                utc = true;
                str = str[0..^4];
            }
            else if (str.EndsWith('Z'))
            {
                utc = true;
                str = str[0..^1];
            }

            if (!DateTime.TryParse(str, out var dt) &&
                !(str.Contains('-') && DateTime.TryParseExact(str, "yyyy-M-d", null, DateTimeStyles.None, out dt)) &&
                !(str.Contains('/') && DateTime.TryParseExact(str, "yyyy/M/d", null, DateTimeStyles.None, out dt)) &&
                !DateTime.TryParseExact(str, "yyyyMMddHHmmss", null, DateTimeStyles.None, out dt) &&
                !DateTime.TryParseExact(str, "yyyyMMdd", null, DateTimeStyles.None, out dt) &&
                !DateTime.TryParse(str, out dt))
            {
                dt = defaultValue;
            }

            // 处理UTC
            if (utc) dt = new DateTime(dt.Ticks, DateTimeKind.Utc);

            return dt;
        }

        // 特殊处理整数，Unix秒，绝对时间差，不考虑UTC时间和本地时间。
        if (value is Int32 k)
        {
            return k >= _maxSeconds || k <= -_maxSeconds ? defaultValue : _dt1970.AddSeconds(k);
        }
        if (value is Int64 m)
        {
            return m >= _maxMilliseconds || m <= -_maxMilliseconds
                ? defaultValue
                : m > 100 * 365 * 24 * 3600L ? _dt1970.AddMilliseconds(m) : _dt1970.AddSeconds(m);
        }

        try
        {
            // 转换接口
            if (value is IConvertible conv) return conv.ToDateTime(null);

            //return Convert.ToDateTime(value);
        }
        catch { }

        // 转字符串再转整数，作为兜底方案
        return ToDateTime(value.ToString(), defaultValue);
    }

    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this Object? value, DateTimeOffset defaultValue = default)
    {
        if (value is DateTimeOffset num) return num;
        if (value == null || value == DBNull.Value) return defaultValue;

#if NET6_0_OR_GREATER
        if (value is DateOnly date) value = date.ToDateTime(TimeOnly.MinValue);
#endif
        if (value is DateTime dateTime) return dateTime;



        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (DateTimeOffset.TryParse(str, out var dt)) return dt;
            return str.Contains('-') && DateTimeOffset.TryParseExact(str, "yyyy-M-d", null, DateTimeStyles.None, out dt) ? dt :
                     str.Contains('/') && DateTimeOffset.TryParseExact(str, "yyyy/M/d", null, DateTimeStyles.None, out dt) ? dt :
                     DateTimeOffset.TryParseExact(str, "yyyyMMddHHmmss", null, DateTimeStyles.None, out dt) ? dt :
                     DateTimeOffset.TryParseExact(str, "yyyyMMdd", null, DateTimeStyles.None, out dt) ? dt : defaultValue;
        }

        // 特殊处理整数，Unix秒，绝对时间差，不考虑UTC时间和本地时间。
        if (value is Int32 k)
        {
            return k >= _maxSeconds || k <= -_maxSeconds ? defaultValue : _dto1970.AddSeconds(k);
        }
        if (value is Int64 m)
        {
            return m >= _maxMilliseconds || m <= -_maxMilliseconds
                ? defaultValue
                : m > 100 * 365 * 24 * 3600L ? _dto1970.AddMilliseconds(m) : _dto1970.AddSeconds(m);
        }

        try
        {
            return Convert.ToDateTime(value);
        }
        catch
        {
            return defaultValue;
        }
    }
    /// <summary>转为时间日期，转换失败时返回最小时间。支持字符串、整数（Unix秒）</summary>
    /// <param name="value">待转换对象</param>
    /// <param name="defaultValue">默认值。待转换对象无效时使用</param>
    /// <returns></returns>
    public static TimeSpan ToTimeSpan(this Object? value, TimeSpan defaultValue = default)
    {
        if (value == null || value == DBNull.Value) return defaultValue;
        if (value is TimeSpan timeSpan1) return timeSpan1;

#if NET6_0_OR_GREATER
        if (value is DateOnly date) value = date.ToDateTime(TimeOnly.MinValue);
        if (value is TimeOnly timeOnly) return timeOnly.ToTimeSpan();
#endif
        if (value is DateTime dateTime1) return TimeSpan.FromTicks(dateTime1.Ticks);
        if (value is DateTimeOffset num) return TimeSpan.FromTicks(num.Ticks);

        // 特殊处理字符串，也是最常见的
        if (value is String str)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return defaultValue;

            if (TimeSpan.TryParse(str, out var dt)) return dt;
        }

        // 特殊处理整数，Unix秒，绝对时间差，不考虑UTC时间和本地时间。
        if (value is Int32 k)
        {
            return TimeSpan.FromSeconds(k);
        }
        if (value is Int64 m)
        {
            return TimeSpan.FromSeconds(m);
        }

        try
        {
            var str1 = value.ToString();
            if (string.IsNullOrEmpty(str1)) return defaultValue;

            if (TimeSpan.TryParse(str1, out var dt)) return dt;
        }
        catch
        {
            return defaultValue;
        }
        return defaultValue;
    }

    /// <summary>去掉时间日期指定位置后面部分，可指定毫秒ms、秒s、分m、小时h、纳秒ns</summary>
    /// <param name="value">时间日期</param>
    /// <param name="format">格式字符串，默认s格式化到秒，ms格式化到毫秒</param>
    /// <returns></returns>
    public static DateTime Trim(this DateTime value, String format = "s")
    {
        return format switch
        {
#if NET8_0_OR_GREATER
            "us" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Microsecond, value.Kind),
            "ns" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Microsecond / 100 * 100, value.Kind),
#endif
            "ms" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind),
            "s" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind),
            "m" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind),
            "h" => new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind),
            _ => value,
        };
    }
    /// <summary>去掉时间日期指定位置后面部分，可指定毫秒ms、秒s、分m、小时h、纳秒ns</summary>
    /// <param name="value">时间日期</param>
    /// <param name="format">格式字符串，默认s格式化到秒，ms格式化到毫秒</param>
    /// <returns></returns>
    public static DateTimeOffset Trim(this DateTimeOffset value, String format = "s")
    {
        return format switch
        {
#if NET8_0_OR_GREATER
            "us" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Microsecond, value.Offset),
            "ns" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Microsecond / 100 * 100, value.Offset),
#endif
            "ms" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Offset),
            "s" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Offset),
            "m" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Offset),
            "h" => new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Offset),
            _ => value,
        };
    }
    /// <summary>清理整数字符串，去掉常见分隔符，替换全角数字为半角数字</summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    private static Int32 TrimNumber(ReadOnlySpan<Char> input, Span<Char> output)
    {
        var idx = 0;

        for (var i = 0; i < input.Length; i++)
        {
            // 去掉逗号分隔符
            var ch = input[i];
            if (ch == ',' || ch == '_' || ch == ' ') continue;

            // 支持前缀正号。Redis响应中就会返回带正号的整数
            if (ch == '+')
            {
                if (idx == 0) continue;
                return 0;
            }

            // 支持负数
            if (ch == '-' && idx > 0) return 0;

            // 全角空格
            if (ch == 0x3000)
                ch = (Char)0x20;
            else if (ch is > (Char)0xFF00 and < (Char)0xFF5F)
                ch = (Char)(input[i] - 0xFEE0);

            // 数字和小数点 以外字符，认为非数字
            if (ch is '.' or '-' or not < '0' and not > '9')
                output[idx++] = ch;
            else
            {
                // 支持科学计数法，e/E后面可以跟正负号，之后至少跟一个数字
                if ((ch is 'e' or 'E') && idx > 0 && i + 2 < input.Length && (input[i + 1] is '+' or '-'))
                {
                    output[idx++] = ch;
                    output[idx++] = input[++i];
                }
                else
                    return 0;
            }
        }

        return idx;
    }
}