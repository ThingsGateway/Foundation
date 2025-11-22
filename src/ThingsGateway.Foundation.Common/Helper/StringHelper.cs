using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
namespace ThingsGateway.Foundation.Common.StringExtension;

public static class StringHelper
{
    private static readonly char[] DotSeparator = new char[] { '.' };
    private static readonly char[] SlashSeparator = new char[] { '/' };
    private static readonly char[] CommaSeparator = new char[] { ',' };
    private static readonly char[] SemicolonSeparator = new char[] { ';' };
    private static readonly char[] HyphenSeparator = new char[] { '-' };

    /// <summary>
    /// 根据<see cref="Type"/> 数据类型转化常见类型，如果不成功，返回false
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeValue(this Type propertyType, string value, out object? objResult)
    {
        if (value == null)
        {
            if (propertyType.IsNullable() || !propertyType.IsValueType)
            {
                objResult = null;
                return true;
            }
        }
        if (propertyType.IsNullable())
        {
            propertyType = propertyType.GetGenericArguments()[0];
        }

        if (propertyType == typeof(bool))
            objResult = value.ToBoolean(false);
        else if (propertyType == typeof(char))
            objResult = char.Parse(value);
        else if (propertyType == typeof(byte))
            objResult = byte.Parse(value);
        else if (propertyType == typeof(sbyte))
            objResult = sbyte.Parse(value);
        else if (propertyType == typeof(short))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = short.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = short.Parse(value);
        }
        else if (propertyType == typeof(ushort))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = ushort.Parse(value);
        }
        else if (propertyType == typeof(int))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = int.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = int.Parse(value);
        }
        else if (propertyType == typeof(uint))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = uint.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = uint.Parse(value);
        }
        else if (propertyType == typeof(long))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = long.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = long.Parse(value);
        }
        else if (propertyType == typeof(ulong))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = ulong.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = ulong.Parse(value);
        }
        else if (propertyType == typeof(float))
            objResult = float.Parse(value);
        else if (propertyType == typeof(double))
            objResult = double.Parse(value);
        else if (propertyType == typeof(decimal))
            objResult = decimal.Parse(value);
        else if (propertyType == typeof(DateTime))
            objResult = DateTime.Parse(value);
        else if (propertyType == typeof(DateTimeOffset))
            objResult = DateTimeOffset.Parse(value);
        else if (propertyType == typeof(string))
            objResult = value;
        else if (propertyType == typeof(IPAddress))
            objResult = IPAddress.Parse(value);
        else if (propertyType.IsEnum)
            objResult = Enum.Parse(propertyType, value);
        else
        {
            objResult = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 根据<see cref="Type"/> 数据类型转化常见类型，如果不成功，返回false
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeStringValue(this Type propertyType, object value, out string? objResult)
    {
        if (propertyType == typeof(bool))
            objResult = value.ToString();
        else if (propertyType == typeof(char))
            objResult = value.ToString();
        else if (propertyType == typeof(byte))
            objResult = value.ToString();
        else if (propertyType == typeof(sbyte))
            objResult = value.ToString();
        else if (propertyType == typeof(short))
            objResult = value.ToString();
        else if (propertyType == typeof(ushort))
            objResult = value.ToString();
        else if (propertyType == typeof(int))
            objResult = value.ToString();
        else if (propertyType == typeof(uint))
            objResult = value.ToString();
        else if (propertyType == typeof(long))
            objResult = value.ToString();
        else if (propertyType == typeof(ulong))
            objResult = value.ToString();
        else if (propertyType == typeof(float))
            objResult = value.ToString();
        else if (propertyType == typeof(double))
            objResult = value.ToString();
        else if (propertyType == typeof(decimal))
            objResult = value.ToString();
        else if (propertyType == typeof(DateTime))
            objResult = value.ToString();
        else if (propertyType == typeof(DateTimeOffset))
            objResult = value.ToString();
        else if (propertyType == typeof(string))
            objResult = value.ToString();
        else if (propertyType == typeof(IPAddress))
            objResult = value.ToString();
        else if (propertyType.IsEnum)
            objResult = value.ToString();
        else
        {
            objResult = null;
            return false;
        }
        return true;
    }

    /// <summary>指定输入是否匹配目标表达式，支持*匹配</summary>
    /// <param name="pattern">匹配表达式</param>
    /// <param name="input">输入字符串</param>
    /// <param name="comparisonType">字符串比较方式</param>
    /// <returns></returns>
    public static Boolean IsMatch(this String pattern, String input, StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (pattern.IsNullOrEmpty()) return false;

        // 单独*匹配所有，即使输入字符串为空
        if (pattern == "*") return true;
        if (input.IsNullOrEmpty()) return false;

        // 普通表达式，直接包含
        var p = pattern.IndexOf('*');
        if (p < 0) return String.Equals(input, pattern, comparisonType);

        // 表达式分组
        var ps = pattern.Split('*');

        // 头尾专用匹配
        if (ps.Length == 2)
        {
            if (p == 0) return input.EndsWith(ps[1], comparisonType);
            if (p == pattern.Length - 1) return input.StartsWith(ps[0], comparisonType);
        }

        // 逐项跳跃式匹配
        p = 0;
        for (var i = 0; i < ps.Length; i++)
        {
            // 最后一组反向匹配
            if (i == ps.Length - 1)
                p = input.LastIndexOf(ps[i], input.Length - 1, input.Length - p, comparisonType);
            else
                p = input.IndexOf(ps[i], p, comparisonType);
            if (p < 0) return false;

            // 第一组必须开头
            if (i == 0 && p > 0) return false;

            p += ps[i].Length;
        }

        // 最后一组*允许不到边界
        if (ps[^1].IsNullOrEmpty()) return p <= input.Length;

        // 最后一组必须结尾
        return p == input.Length;
    }

    /// <summary>字符串转数组</summary>
    /// <param name="value">字符串</param>
    /// <param name="encoding">编码，默认utf-8无BOM</param>
    /// <returns></returns>
    public static Byte[] GetBytes(this String? value, Encoding? encoding = null)
    {
        //if (value == null) return null;
        if (String.IsNullOrEmpty(value)) return [];

        encoding ??= Encoding.UTF8;
        return encoding.GetBytes(value);
    }

    /// <summary>拆分字符串，过滤空格，无效时返回空数组</summary>
    /// <param name="value">字符串</param>
    /// <param name="separators">分组分隔符，默认逗号分号</param>
    /// <returns></returns>
    public static String[] Split(this String? value, params String[] separators)
    {
        //!! netcore3.0中新增Split(String? separator, StringSplitOptions options = StringSplitOptions.None)，优先于StringHelper扩展
        if (value == null || String.IsNullOrEmpty(value)) return [];
        if (separators == null || separators.Length <= 0 || separators.Length == 1 && separators[0].IsNullOrEmpty()) separators = [",", ";"];

        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool StartsWithIgnoreCase(this string? str, string value1, string value2)
    {
        return str.StartsWithIgnoreCase(value1) ||
               str.StartsWithIgnoreCase(value2);
    }

    public static bool StartsWithIgnoreCase(this string? str, string value1, string value2, string value3)
    {
        return str.StartsWithIgnoreCase(value1) ||
               str.StartsWithIgnoreCase(value2) ||
               str.StartsWithIgnoreCase(value3);
    }
    public static bool StartsWithIgnoreCase(this string? str, string value1, string value2, string value3, string value4)
    {
        return str.StartsWithIgnoreCase(value1) ||
               str.StartsWithIgnoreCase(value2) ||
               str.StartsWithIgnoreCase(value3) ||
               str.StartsWithIgnoreCase(value4);
    }
    public static bool StartsWithIgnoreCase(this string? str, string value1, string value2, string value3, string value4, string value5)
    {
        return str.StartsWithIgnoreCase(value1) ||
               str.StartsWithIgnoreCase(value2) ||
               str.StartsWithIgnoreCase(value3) ||
               str.StartsWithIgnoreCase(value4) ||
               str.StartsWithIgnoreCase(value5);
    }
    /// <summary>确保字符串以指定的另一字符串开始，不区分大小写</summary>
    /// <param name="str">字符串</param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static String EnsureStart(this String? str, String start)
    {
        if (String.IsNullOrEmpty(start)) return str + string.Empty;
        if (String.IsNullOrEmpty(str) || str == null) return start + string.Empty;

        if (str.StartsWith(start, StringComparison.OrdinalIgnoreCase)) return str;

        return start + str;
    }

    /// <summary>确保字符串以指定的另一字符串结束，不区分大小写</summary>
    /// <param name="str">字符串</param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static String EnsureEnd(this String? str, String end)
    {
        if (String.IsNullOrEmpty(end)) return str + string.Empty;
        if (String.IsNullOrEmpty(str) || str == null) return end + string.Empty;

        if (str.EndsWith(end, StringComparison.OrdinalIgnoreCase)) return str;

        return str + end;
    }

    /// <summary>从当前字符串开头移除另一字符串以及之前的部分</summary>
    /// <param name="str">当前字符串</param>
    /// <param name="starts">另一字符串</param>
    /// <returns></returns>
    public static String CutStart(this String str, params String[] starts)
    {
        if (str.IsNullOrEmpty()) return str;
        if (starts == null || starts.Length <= 0 || starts[0].IsNullOrEmpty()) return str;

        for (var i = 0; i < starts.Length; i++)
        {
            var p = str.IndexOf(starts[i], StringComparison.Ordinal);
            if (p >= 0)
            {
                str = str[(p + starts[i].Length)..];
                if (str.IsNullOrEmpty()) break;
            }
        }
        return str;
    }

    /// <summary>根据最大长度截取字符串，并允许以指定空白填充末尾</summary>
    /// <param name="str">字符串</param>
    /// <param name="maxLength">截取后字符串的最大允许长度，包含后面填充</param>
    /// <param name="pad">需要填充在后面的字符串，比如几个圆点</param>
    /// <returns></returns>
    public static String Cut(this String str, Int32 maxLength, String? pad = null)
    {
        if (String.IsNullOrEmpty(str) || maxLength <= 0 || str.Length < maxLength) return str;

        // 计算截取长度
        var len = maxLength;
        if (pad != null && !String.IsNullOrEmpty(pad)) len -= pad.Length;
        if (len <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength));

        return str[..len] + pad;
    }

    /// <summary>从当前字符串结尾移除另一字符串以及之后的部分</summary>
    /// <param name="str">当前字符串</param>
    /// <param name="ends">另一字符串</param>
    /// <returns></returns>
    public static String CutEnd(this String str, params String[] ends)
    {
        if (String.IsNullOrEmpty(str)) return str;
        if (ends == null || ends.Length <= 0 || String.IsNullOrEmpty(ends[0])) return str;

        for (var i = 0; i < ends.Length; i++)
        {
            var p = str.LastIndexOf(ends[i], StringComparison.Ordinal);
            if (p >= 0)
            {
                str = str[..p];
                if (String.IsNullOrEmpty(str)) break;
            }
        }
        return str;
    }
#if NETFRAMEWORK || NETSTANDARD2_0
    public static Boolean Contains(this String value, Char inputChar) => value.IndexOf(inputChar) >= 0;

    public static String[] Split(this String value, Char separator, StringSplitOptions options = StringSplitOptions.None) => value.Split(new Char[] { separator }, options);
#endif

    public static bool HasValue([NotNullWhen(true)] this string? thisValue)
    {
        return !string.IsNullOrEmpty(thisValue);
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? thisValue)
    {
        return string.IsNullOrEmpty(thisValue);
    }
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? thisValue)
    {
        return string.IsNullOrWhiteSpace(thisValue);
    }

#if !NET6_0_OR_GREATER
    public static Boolean EndsWith(this String? value, char charValue)
    {
        return value?.EndsWith(charValue.ToString()) ?? false;
    }
    public static Boolean StartsWith(this String? value, char charValue)
    {
        return value?.StartsWith(charValue.ToString()) ?? false;
    }

    public static Boolean Contains(this String? value, string stringValue, StringComparison stringComparison)
    {
        return value?.IndexOf(stringValue, stringComparison) >= 0;
    }

#endif

    public static bool EndsWithIgnoreCase(this string? str, string? value)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;
        if (str == null || String.IsNullOrEmpty(str)) return false;
        return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }


    public static bool EndsWithIgnoreCase(this string? str, string value1, string value2)
    {
        return str.EndsWithIgnoreCase(value1) ||
               str.EndsWithIgnoreCase(value2);
    }

    public static bool EndsWithIgnoreCase(this string? str, string value1, string value2, string value3)
    {
        return str.EndsWithIgnoreCase(value1) ||
               str.EndsWithIgnoreCase(value2) ||
               str.EndsWithIgnoreCase(value3);
    }
    public static bool EndsWithIgnoreCase(this string? str, string value1, string value2, string value3, string value4)
    {
        return str.EndsWithIgnoreCase(value1) ||
               str.EndsWithIgnoreCase(value2) ||
               str.EndsWithIgnoreCase(value3) ||
               str.EndsWithIgnoreCase(value4);
    }
    public static bool EndsWithIgnoreCase(this string? str, string value1, string value2, string value3, string value4, string value5)
    {
        return str.EndsWithIgnoreCase(value1) ||
               str.EndsWithIgnoreCase(value2) ||
               str.EndsWithIgnoreCase(value3) ||
               str.EndsWithIgnoreCase(value4) ||
               str.EndsWithIgnoreCase(value5);
    }

    /// <summary>忽略大小写的字符串结束比较，判断是否以任意一个待比较字符串结束</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean EndsWithIgnoreCase(this String? value, params String?[] strs)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;

        foreach (var item in strs)
        {
            if (item != null && value.EndsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>忽略大小写的字符串结束比较，判断是否以任意一个待比较字符串结束</summary>
    /// <param name="value">字符串</param>
    /// <param name="c">待比较字符</param>
    /// <returns></returns>
    public static Boolean EndsWithIgnoreCase(this String? value, char c)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;

        return value.EndsWith(c);
    }
    public static string RegexReplaceIgnoreCase(string input, string pattern, string replacement)
    {
        return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
    }

    public static string ReplaceIgnoreCase(this String? value, char oldValue, char newValue)
    {
        if (value == null || String.IsNullOrEmpty(value)) return string.Empty;

        return value.Replace(oldValue, newValue);
    }

#if NET6_0_OR_GREATER||NETSTANDARD2_1_OR_GREATER
    public static string ReplaceIgnoreCase(this String? value, string oldValue, string newValue)
    {
        if (value == null || String.IsNullOrEmpty(value)) return string.Empty;

        return value.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
    }

#else
    /// <summary>
    /// 忽略大小写的字符串替换（兼容 .NET Framework 4.6.2）
    /// </summary>
    /// <param name="value">原始字符串</param>
    /// <param name="oldValue">要替换的目标字符串</param>
    /// <param name="newValue">新的字符串</param>
    /// <returns>返回替换后的字符串</returns>
    public static string ReplaceIgnoreCase(this string value, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (string.IsNullOrEmpty(oldValue))
            return value;

        // 使用 StringComparison.OrdinalIgnoreCase 模拟替换
        var sb = new StringBuilder(value.Length);
        int previousIndex = 0;
        int index = value.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);

        while (index != -1)
        {
            // 追加前面未替换部分
            sb.Append(value, previousIndex, index - previousIndex);
            // 追加新值
            sb.Append(newValue);
            // 跳过已替换部分
            previousIndex = index + oldValue.Length;
            // 查找下一个匹配
            index = value.IndexOf(oldValue, previousIndex, StringComparison.OrdinalIgnoreCase);
        }

        // 追加最后剩余部分
        sb.Append(value, previousIndex, value.Length - previousIndex);
        return sb.ToString();
    }
#endif
    /// <summary>拆分字符串成为整型数组，默认逗号分号分隔，无效时返回空数组</summary>
    /// <remarks>过滤空格、过滤无效、不过滤重复</remarks>
    /// <param name="value">字符串</param>
    /// <param name="separators">分组分隔符，默认逗号分号</param>
    /// <returns></returns>
    public static Int32[] SplitAsInt(this String? value, params String[] separators)
    {
        if (value == null || String.IsNullOrEmpty(value)) return [];
        if (separators == null || separators.Length == 0) separators = [",", ";"];

        var ss = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        using var list = new ValueListBuilder<Int32>();
        foreach (var item in ss)
        {
            if (!Int32.TryParse(item.Trim(), out var id)) continue;

            list.Add(id);
        }

        return list.AsSpan().ToArray();
    }


    /// <summary>拆分字符串成为不区分大小写的可空名值字典。逗号分组，等号分隔</summary>
    /// <param name="value">字符串</param>
    /// <param name="nameValueSeparator">名值分隔符，默认等于号</param>
    /// <param name="separator">分组分隔符，默认分号</param>
    /// <param name="trimQuotation">去掉括号</param>
    /// <returns></returns>
    public static IDictionary<String, String> SplitAsDictionary(this String? value, String nameValueSeparator = "=", String separator = ";", Boolean trimQuotation = false)
    {
        var dic = new NullableDictionary<String, String>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(value))
            return dic;

        if (string.IsNullOrEmpty(nameValueSeparator)) nameValueSeparator = "=";
        //if (separator == null || separator.Length <= 0) separator = new String[] { ",", ";" };

        var ss = value!.Split([separator], StringSplitOptions.RemoveEmptyEntries);
        if (ss == null || ss.Length == 0) return dic;

        var k = 0;
        foreach (var item in ss)
        {
            // 如果分隔符是 \u0001，则必须使用Ordinal，否则无法分割直接返回0。在RocketMQ中有这种情况
            var p = item.IndexOf(nameValueSeparator, StringComparison.Ordinal);
            if (p <= 0)
            {
                dic[$"[{k}]"] = item;
                k++;
                continue;
            }

            var key = item[..p].Trim();
            var val = item[(p + nameValueSeparator.Length)..].Trim();

            // 处理单引号双引号
            if (trimQuotation && !string.IsNullOrEmpty(val))
            {
                if (val[0] == '\'' && val[^1] == '\'') val = val.Trim('\'');
                if (val[0] == '"' && val[^1] == '"') val = val.Trim('"');
            }

            k++;
            //dic[key] = val;
#if NETFRAMEWORK || NETSTANDARD2_0
            if (!dic.ContainsKey(key)) dic.Add(key, val);
#else
            dic.TryAdd(key, val);
#endif
        }

        return dic;
    }


    /// <summary>从字符串中检索子字符串，在指定头部字符串之后，指定尾部字符串之前</summary>
    /// <remarks>常用于截取xml某一个元素等操作</remarks>
    /// <param name="str">目标字符串</param>
    /// <param name="after">头部字符串，在它之后</param>
    /// <param name="before">尾部字符串，在它之前</param>
    /// <param name="startIndex">搜索的开始位置</param>
    /// <param name="positions">位置数组，两个元素分别记录头尾位置</param>
    /// <returns></returns>
    public static String Substring(this String str, String? after, String? before = null, Int32 startIndex = 0, Int32[]? positions = null)
    {
        if (String.IsNullOrEmpty(str)) return str;
        if (String.IsNullOrEmpty(after) && String.IsNullOrEmpty(before)) return str;

        /*
         * 1，只有start，从该字符串之后部分
         * 2，只有end，从开头到该字符串之前
         * 3，同时start和end，取中间部分
         */

        var p = -1;
        if (!string.IsNullOrEmpty(after))
        {
            p = str.IndexOf(after, startIndex);
            if (p < 0) return String.Empty;
            p += after!.Length;

            // 记录位置
            if (positions?.Length > 0) positions[0] = p;
        }

        if (String.IsNullOrEmpty(before)) return str[p..];

        var f = str.IndexOf(before, p >= 0 ? p : startIndex);
        if (f < 0) return String.Empty;

        // 记录位置
        if (positions?.Length > 1) positions[1] = f;

        if (p >= 0)
            return str[p..f];
        else
            return str[..f];
    }
    /// <summary>修剪不可见字符。仅修剪ASCII，不包含Unicode</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string TrimInvisible(this ReadOnlySpan<char> value)
    {
        if (value.Length <= 0) return string.Empty;

        using var builder = new ValueStringBuilder(value.Length);

        for (var i = 0; i < value.Length; i++)
        {
            // 可见字符。ASCII码中，第0～31号及第127号(共33个)是控制字符或通讯专用字符
            if (value[i] is > (Char)31 and not (Char)127)
                builder.Append(value[i]);
        }

        return builder.ToString();
    }


    /// <summary>
    /// 根据给定的Bcd值字符串和Bcd格式返回对应的字节数组
    /// </summary>
    /// <param name="value">Bcd值字符串</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] GetBytesByBCD(this string value, BcdFormatEnum format)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }

        int length = (value.Length + 1) / 2;
        byte[] bytesFromBcd = new byte[length];

        for (int index = 0; index < length; ++index)
        {
            byte highNibble = (byte)(GetBcdCodeFromChar(value[2 * index], format) << 4);
            byte lowNibble = 0;

            if ((2 * index) + 1 < value.Length)
            {
                lowNibble = GetBcdCodeFromChar(value[(2 * index) + 1], format);
            }

            bytesFromBcd[index] = (byte)(highNibble | lowNibble);
        }

        return bytesFromBcd;
    }

    /// <summary>
    /// 获取Bcd值
    /// </summary>
    internal static byte GetBcdCodeFromChar(char value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C5421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C2421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 11,
                '6' => 12,
                '7' => 13,
                '8' => 14,
                '9' => 15,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C3 => value switch
            {
                '0' => 3,
                '1' => 4,
                '2' => 5,
                '3' => 6,
                '4' => 7,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.Gray => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 3,
                '3' => 2,
                '4' => 6,
                '5' => 7,
                '6' => 5,
                '7' => 4,
                '8' => 12,
                '9' => 8,
                _ => byte.MaxValue,
            },
            _ => byte.MaxValue,
        };
    }

    /// <summary>
    /// 获取Bcd值
    /// </summary>
    internal static string GetBcdFromByte(int value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                5 => "5",
                6 => "6",
                7 => "7",
                8 => "8",
                9 => "9",
                _ => "*",
            },
            BcdFormatEnum.C5421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.C2421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                11 => "5",
                12 => "6",
                13 => "7",
                14 => "8",
                15 => "9",
                _ => "*",
            },
            BcdFormatEnum.C3 => value switch
            {
                3 => "0",
                4 => "1",
                5 => "2",
                6 => "3",
                7 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.Gray => value switch
            {
                0 => "0",
                1 => "1",
                2 => "3",
                3 => "2",
                4 => "7",
                5 => "6",
                6 => "4",
                7 => "5",
                8 => "9",
                12 => "8",
                _ => "*",
            },
            _ => "*",
        };
    }

    /// <summary>
    /// 将十六进制字符串转换为对应的字节数组
    /// </summary>
    /// <param name="hex">输入的十六进制字符串</param>
    /// <returns>转换后的字节数组</returns>
    public static Memory<byte> HexStringToBytes(this string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Memory<byte>.Empty;

        int len = hex.Length / 2;
        var result = new byte[len];
        int byteIndex = 0;

        for (int i = 0; i < hex.Length - 1; i += 2)
        {
            int hi = GetHexCharIndex(hex[i]);
            int lo = GetHexCharIndex(hex[i + 1]);

            if (hi >= 0 && lo >= 0 && hi < 0x10 && lo < 0x10)
            {
                result[byteIndex++] = (byte)((hi << 4) | lo);
            }
            else
            {
                i--;
                continue;
            }
        }

        return new Memory<byte>(result, 0, byteIndex);
    }

    private static byte GetHexCharIndex(char ch)
    {
        return ch switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 0x0a,
            'B' or 'b' => 0x0b,
            'C' or 'c' => 0x0c,
            'D' or 'd' => 0x0d,
            'E' or 'e' => 0x0e,
            'F' or 'f' => 0x0f,
            _ => 0x10,
        };
    }

    /// <summary>
    /// 根据英文逗号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitByComma(this string? str)
    {
        return str?.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// 根据-符号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitByHyphen(this string? str)
    {
        return str?.Split(HyphenSeparator, StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// 根据英文分号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringBySemicolon(this string? str)
    {
        return str?.Split(SemicolonSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据英文小数点进行分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringByDelimiter(this string? str)
    {
        return str?.Split(DotSeparator, StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// 根据斜杠进行分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringBySlash(this string? str)
    {
        return str?.Split(SlashSeparator, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>把一个列表组合成为一个字符串，默认逗号分隔</summary>
    /// <param name="value"></param>
    /// <param name="separator">组合分隔符，默认逗号</param>
    /// <param name="func">把对象转为字符串的委托</param>
    /// <returns></returns>
    public static String Join<T>(this IEnumerable<T> value, String separator = ",", Func<T, string?>? func = null)
    {
        using var sb = new ValueStringBuilder();
        if (value != null)
        {
            foreach (var item in value)
            {
                if (!String.IsNullOrEmpty(separator))
                {
                    if (sb.Length > 0)
                        sb.Append(separator);
                }

                if (func != null)
                    sb.Append(func(item));
                else
                    sb.Append(item is string str ? str : item?.ToString());
            }
        }
        return sb.ToString();
    }


    /// <summary>
    /// 忽略大小写的字符串开始比较，判断是否与任意一个待比较字符开始。
    /// </summary>
    /// <param name="value">字符串</param>
    /// <param name="c">待比较字符</param>
    /// <returns></returns>
    public static bool StartsWithIgnoreCase(this string? value, Char c)
    {
        if (string.IsNullOrEmpty(value)) return false;

        return Char.ToUpperInvariant(value![0]) == Char.ToUpperInvariant(c);
    }

    /// <summary>忽略大小写的字符串开始比较，判断是否与任意一个待比较字符串开始</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean StartsWithIgnoreCase(this String? value, params String?[] strs)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;

        foreach (var item in strs)
        {
            if (!String.IsNullOrEmpty(item) && value.StartsWith(item, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
    /// <summary>忽略大小写的字符串开始比较，判断是否与任意一个待比较字符串开始</summary>
    /// <param name="value">字符串</param>
    /// <param name="str">待比较字符串</param>
    /// <returns></returns>
    public static Boolean StartsWithIgnoreCase(this String? value, String str)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;

        return !String.IsNullOrEmpty(str) && value.StartsWith(str, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean EqualIgnoreCase(this String? value, String? strs)
    {
        return String.Equals(value, strs, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean ContainsIgnoreCase(this String? value, String? strs)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;
        if (strs == null || String.IsNullOrEmpty(strs)) return false;
        return value.Contains(strs, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean ContainsIgnoreCase(this String? value, char strs)
    {
        if (value == null || String.IsNullOrEmpty(value)) return false;
        return value.Contains(strs);
    }
    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean ContainsIgnoreCase(this IEnumerable<string>? value, String? strs)
    {
        if (value == null) return false;
        if (strs == null || String.IsNullOrEmpty(strs)) return false;
        return value.Contains(strs, StringComparer.OrdinalIgnoreCase);
    }



    public static bool ContainsIgnoreCase(this string? str, string value1, string value2)
    {
        return str.ContainsIgnoreCase(value1) ||
               str.ContainsIgnoreCase(value2);
    }

    public static bool ContainsIgnoreCase(this string? str, string value1, string value2, string value3)
    {
        return str.ContainsIgnoreCase(value1) ||
               str.ContainsIgnoreCase(value2) ||
               str.ContainsIgnoreCase(value3);
    }
    public static bool ContainsIgnoreCase(this string? str, string value1, string value2, string value3, string value4)
    {
        return str.ContainsIgnoreCase(value1) ||
               str.ContainsIgnoreCase(value2) ||
               str.ContainsIgnoreCase(value3) ||
               str.ContainsIgnoreCase(value4);
    }
    public static bool ContainsIgnoreCase(this string? str, string value1, string value2, string value3, string value4, string value5)
    {
        return str.ContainsIgnoreCase(value1) ||
               str.ContainsIgnoreCase(value2) ||
               str.ContainsIgnoreCase(value3) ||
               str.ContainsIgnoreCase(value4) ||
               str.ContainsIgnoreCase(value5);
    }
    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="strs">字符串</param>
    /// <param name="value">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean ContainsIgnoreCase(this String? strs, params string[]? value)
    {
        if (value == null) return false;
        if (strs == null || String.IsNullOrEmpty(strs)) return false;
        foreach (var item in value)
        {
            if (strs.Contains(item, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }



    public static bool EqualIgnoreCase(this string? str, string value1, string value2)
    {
        return str.EqualIgnoreCase(value1) ||
               str.EqualIgnoreCase(value2);
    }

    public static bool EqualIgnoreCase(this string? str, string value1, string value2, string value3)
    {
        return str.EqualIgnoreCase(value1) ||
               str.EqualIgnoreCase(value2) ||
               str.EqualIgnoreCase(value3);
    }
    public static bool EqualIgnoreCase(this string? str, string value1, string value2, string value3, string value4)
    {
        return str.EqualIgnoreCase(value1) ||
               str.EqualIgnoreCase(value2) ||
               str.EqualIgnoreCase(value3) ||
               str.EqualIgnoreCase(value4);
    }
    public static bool EqualIgnoreCase(this string? str, string value1, string value2, string value3, string value4, string value5)
    {
        return str.EqualIgnoreCase(value1) ||
               str.EqualIgnoreCase(value2) ||
               str.EqualIgnoreCase(value3) ||
               str.EqualIgnoreCase(value4) ||
               str.EqualIgnoreCase(value5);
    }

    /// <summary>忽略大小写的字符串相等比较，判断是否与任意一个待比较字符串相等</summary>
    /// <param name="value">字符串</param>
    /// <param name="strs">待比较字符串数组</param>
    /// <returns></returns>
    public static Boolean EqualIgnoreCase(this String? value, params String?[] strs)
    {
        foreach (var item in strs)
        {
            if (String.Equals(value, item, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

#if NETFRAMEWORK || NETSTANDARD2_0
    /// <summary>拆分字符串，过滤空格，无效时返回空数组</summary>
    /// <param name="value">字符串</param>
    /// <param name="separators">分组分隔符，默认逗号分号</param>
    /// <returns></returns>
    public static String[] Split(this String? value, string separators)
    {
        //!! netcore3.0中新增Split(String? separator, StringSplitOptions options = StringSplitOptions.None)，优先于StringHelper扩展
        if (value == null || String.IsNullOrEmpty(value)) return [];

        return value.Split(separators.ToCharArray());
    }
#endif

}