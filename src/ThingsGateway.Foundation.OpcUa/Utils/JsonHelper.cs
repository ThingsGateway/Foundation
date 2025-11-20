using Newtonsoft.Json;

using Opc.Ua.Buffers;

using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ThingsGateway.Foundation.OpcUa
{
    internal static class JsonHelper
    {
        internal static int CalculateActualValueRank(this JsonNode node)
        {
            if (node is not JsonArray array)
                return -1;

            int numDimensions = 0;
            JsonNode current = node;

            while (current is JsonArray arr)
            {
                numDimensions++;

                if (arr.Count == 0)
                    break;

                // 进入下一个维度
                current = arr[0];
            }

            return numDimensions;
        }



        #region JSON Builders
        internal static Type GetSystemType(JsonNode node)
        {
            if (node == null)
                return typeof(string);

            switch (node)
            {
                case JsonObject:
                    return typeof(string);

                case JsonArray:
                    return typeof(Array);

                case JsonValue jsonValue:
                    return GetSystemTypeFromJsonValue(jsonValue);

                default:
                    return typeof(string);
            }
        }

        private static Type GetSystemTypeFromJsonValue(JsonValue jsonValue)
        {
            if (!jsonValue.TryGetValue<object>(out var val) || val == null)
                return typeof(string);

            switch (val)
            {
                case bool:
                    return typeof(bool);

                case sbyte:
                case byte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                    return typeof(long);

                case float:
                case double:
                case decimal:
                    return typeof(float);

                case string:
                    return typeof(string);

                case DateTime:
                    return typeof(DateTime);

                case Guid:
                    return typeof(Guid);

                case byte[]:
                    return typeof(byte[]);

                case Uri:
                    return typeof(Uri);

                case TimeSpan:
                    return typeof(TimeSpan);

                default:
                    return typeof(string);
            }
        }

        internal static string BuildSimpleValueJson(JsonNode body)
        {

            using var ms = new ArrayPoolBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            body.WriteTo(writer);
            writer.WriteEndObject();
            writer.Flush();

            var data = ms.GetSpan();
            return JsonHelper.GetString(Encoding.UTF8, data);

        }


        internal static string BuildVariantJson(NodeId typeId, JsonNode body)
        {
            using var ms = new ArrayPoolBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            writer.WriteStartObject();

            writer.WritePropertyName("Type");
            System.Text.Json.JsonSerializer.Serialize(writer, typeId.Identifier);

            writer.WritePropertyName("Body");
            body.WriteTo(writer);

            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.Flush();
            var data = ms.GetSpan();
            return JsonHelper.GetString(Encoding.UTF8, data);

        }

        internal static string BuildExtensionObjectJson(NodeId dataTypeId, JsonNode body)
        {
            using var ms = new ArrayPoolBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            writer.WriteStartObject();

            writer.WritePropertyName("TypeId");
            writer.WriteStartObject();
            writer.WritePropertyName("Id");
            System.Text.Json.JsonSerializer.Serialize(writer, dataTypeId.Identifier);
            writer.WritePropertyName("Namespace");
            writer.WriteNumberValue(dataTypeId.NamespaceIndex);
            writer.WriteEndObject();

            writer.WritePropertyName("Body");
            body.WriteTo(writer);

            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.Flush();
            var data = ms.GetSpan();
            return JsonHelper.GetString(Encoding.UTF8, data);

        }

        #endregion



        /// <summary>获取字节数组的字符串</summary>
        public static unsafe String GetString(Encoding encoding, ReadOnlySpan<Byte> bytes)
        {
            if (bytes.IsEmpty) return String.Empty;

#if NET452
        return encoding.GetString(bytes.ToArray());
#else
            fixed (Byte* bytes2 = &MemoryMarshal.GetReference(bytes))
            {
                return encoding.GetString(bytes2, bytes.Length);
            }
#endif
        }
        internal static object DecodeRawData(JsonDecoder decoder, BuiltInType builtInType, int ValueRank, string fieldName)
        {
            if (builtInType != 0)
            {
                if (ValueRank == ValueRanks.Scalar)
                {
                    var idx = (int)builtInType;
                    if (idx >= 0 && idx < _scalarHandlers.Length)
                    {
                        var handler = _scalarHandlers[idx];
                        if (handler != null)
                            return handler(decoder, fieldName);
                    }
                }
                if (ValueRank >= ValueRanks.OneDimension)
                {
                    return decoder.ReadArray(fieldName, ValueRank, builtInType);
                }
            }
            return null;
        }


        #region Encoder Utilities

        internal static JsonEncoder CreateEncoder(
            IServiceMessageContext context,
            Stream stream,
            bool useReversibleEncoding = false,
            bool topLevelIsArray = false,
            bool includeDefaultValues = true,
            bool includeDefaultNumbers = true
        )
        {
            return new JsonEncoder(context, useReversibleEncoding, topLevelIsArray, stream)
            {
                IncludeDefaultValues = includeDefaultValues,
                IncludeDefaultNumberValues = includeDefaultNumbers
            };
        }

        internal static void Encode(JsonEncoder encoder, BuiltInType builtInType, string fieldName, object value)
        {
            bool isArray = (value?.GetType().IsArray ?? false) && (builtInType != BuiltInType.ByteString);
            bool isCollection = (value is IList) && (builtInType != BuiltInType.ByteString);
            if (!isArray && !isCollection)
            {
                switch (builtInType)
                {
                    case BuiltInType.Null:
                        encoder.WriteVariant(fieldName, new Variant(value));
                        return;

                    // 基本数字类型 — 优先直接类型判断，避免 Convert.* 的慢路径
                    case BuiltInType.Boolean:
                        encoder.WriteBoolean(fieldName, value is bool b ? b : ConvertToBooleanFast(value));
                        return;
                    case BuiltInType.SByte:
                        encoder.WriteSByte(fieldName, value is sbyte sb ? sb : ConvertToSByteFast(value));
                        return;
                    case BuiltInType.Byte:
                        encoder.WriteByte(fieldName, value is byte bt ? bt : ConvertToByteFast(value));
                        return;
                    case BuiltInType.Int16:
                        encoder.WriteInt16(fieldName, value is short s ? s : ConvertToInt16Fast(value));
                        return;
                    case BuiltInType.UInt16:
                        encoder.WriteUInt16(fieldName, value is ushort us ? us : ConvertToUInt16Fast(value));
                        return;
                    case BuiltInType.Int32:
                        if (value is int vi) { encoder.WriteInt32(fieldName, vi); return; }
                        encoder.WriteInt32(fieldName, ParseInt32Fast(value));
                        return;
                    case BuiltInType.UInt32:
                        encoder.WriteUInt32(fieldName, value is uint u32 ? u32 : (uint)ParseInt64Fast(value));
                        return;
                    case BuiltInType.Int64:
                        if (value is long vl) { encoder.WriteInt64(fieldName, vl); return; }
                        encoder.WriteInt64(fieldName, ParseInt64Fast(value));
                        return;
                    case BuiltInType.UInt64:
                        if (value is ulong vul) { encoder.WriteUInt64(fieldName, vul); return; }
                        encoder.WriteUInt64(fieldName, (ulong)ParseUInt64Fast(value));
                        return;
                    case BuiltInType.Float:
                        if (value is float vf) { encoder.WriteFloat(fieldName, vf); return; }
                        encoder.WriteFloat(fieldName, ParseFloatFast(value));
                        return;
                    case BuiltInType.Double:
                    case BuiltInType.Number:
                        if (value is double vd) { encoder.WriteDouble(fieldName, vd); return; }
                        encoder.WriteDouble(fieldName, ParseDoubleFast(value));
                        return;
                    case BuiltInType.Integer:
                        encoder.WriteInt64(fieldName, ParseInt64Fast(value));
                        return;
                    case BuiltInType.UInteger:
                        encoder.WriteUInt64(fieldName, (ulong)ParseUInt64Fast(value));
                        return;
                    case BuiltInType.String:
                        encoder.WriteString(fieldName, value?.ToString());
                        return;
                    case BuiltInType.DateTime:
                        if (value is DateTime dt) { encoder.WriteDateTime(fieldName, dt); return; }
                        encoder.WriteDateTime(fieldName, Convert.ToDateTime(value, CultureInfo.InvariantCulture));
                        return;

                    case BuiltInType.Guid:
                        encoder.WriteGuid(fieldName, (Uuid)value);
                        return;
                    case BuiltInType.ByteString:
                        encoder.WriteByteString(fieldName, (byte[])value);
                        return;
                    case BuiltInType.XmlElement:
                        encoder.WriteXmlElement(fieldName, (System.Xml.XmlElement)value);
                        return;
                    case BuiltInType.NodeId:
                        encoder.WriteNodeId(fieldName, (NodeId)value);
                        return;
                    case BuiltInType.ExpandedNodeId:
                        encoder.WriteExpandedNodeId(fieldName, (ExpandedNodeId)value);
                        return;
                    case BuiltInType.StatusCode:
                        encoder.WriteStatusCode(fieldName, (StatusCode)value);
                        return;
                    case BuiltInType.QualifiedName:
                        encoder.WriteQualifiedName(fieldName, (QualifiedName)value);
                        return;
                    case BuiltInType.LocalizedText:
                        encoder.WriteLocalizedText(fieldName, (LocalizedText)value);
                        return;
                    case BuiltInType.ExtensionObject:
                        encoder.WriteExtensionObject(fieldName, (ExtensionObject)value);
                        return;
                    case BuiltInType.DataValue:
                        encoder.WriteDataValue(fieldName, (DataValue)value);
                        return;
                    case BuiltInType.Enumeration:
                        if (value?.GetType().IsEnum == true)
                        {
                            encoder.WriteEnumerated(fieldName, (Enum)value);
                        }
                        else
                        {
                            encoder.WriteEnumerated(fieldName, (Enumeration)value);
                        }
                        return;
                    case BuiltInType.Variant:
                        encoder.WriteVariant(fieldName, new Variant(value));
                        return;
                    case BuiltInType.DiagnosticInfo:
                        encoder.WriteDiagnosticInfo(fieldName, (DiagnosticInfo)value);
                        return;
                }
            }
            else
            {
                // 数组/集合
                Array c = value as Array;
                encoder.WriteArray(fieldName, c, c?.Rank ?? 1, builtInType);
            }
        }

        #endregion

        #region JSON Builders

        internal static string BuildSimpleValueJson(JToken body)
        {
            var sb = RentStringBuilder();
            try
            {
                using var sw = new StringWriter(sb);
                using var writer = new JsonTextWriter(sw) { Formatting = Formatting.None };
                writer.WriteStartObject();
                writer.WritePropertyName("Value");
                body.WriteTo(writer);
                writer.WriteEndObject();
                writer.Flush();
                return sb.ToString();
            }
            finally
            {
                ReturnStringBuilder(sb);
            }
        }

        internal static string BuildVariantJson(NodeId typeId, JToken body)
        {
            var sb = RentStringBuilder();
            try
            {
                using var sw = new StringWriter(sb);
                using var writer = new JsonTextWriter(sw) { Formatting = Formatting.None };

                writer.WriteStartObject();
                writer.WritePropertyName("Value");
                writer.WriteStartObject();

                writer.WritePropertyName("Type");
                writer.WriteValue(typeId.Identifier);

                writer.WritePropertyName("Body");
                body.WriteTo(writer);

                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.Flush();
                return sb.ToString();
            }
            finally
            {
                ReturnStringBuilder(sb);
            }
        }

        internal static string BuildExtensionObjectJson(NodeId dataTypeId, JToken body)
        {
            var sb = RentStringBuilder();
            try
            {
                using var sw = new StringWriter(sb);
                using var writer = new JsonTextWriter(sw) { Formatting = Formatting.None };

                writer.WriteStartObject();
                writer.WritePropertyName("Value");
                writer.WriteStartObject();

                writer.WritePropertyName("TypeId");
                writer.WriteStartObject();
                writer.WritePropertyName("Id");
                writer.WriteValue(dataTypeId.Identifier);
                writer.WritePropertyName("Namespace");
                writer.WriteValue(dataTypeId.NamespaceIndex);
                writer.WriteEndObject();

                writer.WritePropertyName("Body");
                body.WriteTo(writer);

                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.Flush();
                return sb.ToString();
            }
            finally
            {
                ReturnStringBuilder(sb);
            }
        }

        #endregion



        #region Fast Convert Helpers

        private static bool ConvertToBooleanFast(object value)
        {
            if (value == null) return false;
            if (value is bool b) return b;
            var s = value as string;
            if (s != null) return bool.TryParse(s, out var r) && r;
            try { return Convert.ToBoolean(value); } catch { return false; }
        }

        private static sbyte ConvertToSByteFast(object value)
        {
            if (value is sbyte sb) return sb;
            if (value is byte b) return (sbyte)b;
            if (value is string s && s.Length > 0 && s != "0")
            {
                if (sbyte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            }
            return (sbyte)Convert.ToInt32(value);
        }

        private static byte ConvertToByteFast(object value)
        {
            if (value is byte b) return b;
            if (value is sbyte sb) return (byte)sb;
            if (value is string s && byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToByte(value);
        }

        private static short ConvertToInt16Fast(object value)
        {
            if (value is short s) return s;
            if (value is int i) return (short)i;
            if (value is string ss && short.TryParse(ss, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToInt16(value);
        }

        private static ushort ConvertToUInt16Fast(object value)
        {
            if (value is ushort us) return us;
            if (value is int i) return (ushort)i;
            if (value is string ss && ushort.TryParse(ss, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToUInt16(value);
        }

        private static int ParseInt32Fast(object value)
        {
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToInt32(value);
        }

        private static long ParseInt64Fast(object value)
        {
            if (value is long l) return l;
            if (value is int i) return i;
            if (value is string s && long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToInt64(value);
        }

        private static ulong ParseUInt64Fast(object value)
        {
            if (value is ulong ul) return ul;
            if (value is long l) return (ulong)l;
            if (value is string s && ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToUInt64(value);
        }

        private static float ParseFloatFast(object value)
        {
            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is string s && float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToSingle(value);
        }

        private static double ParseDoubleFast(object value)
        {
            if (value is double d) return d;
            if (value is float f) return f;
            if (value is string s && double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var r)) return r;
            return Convert.ToDouble(value);
        }

        #endregion

        #region DecodeRawData with delegate table (fast)

        private delegate object ReadHandler(JsonDecoder decoder, string fieldName);

        private static readonly ReadHandler[] _scalarHandlers;

        static JsonHelper()
        {
            // 初始化 handler 表，索引按 BuiltInType 枚举的 int 值
            var max = Enum.GetValues(typeof(BuiltInType)).Cast<int>().Max() + 1;
            _scalarHandlers = new ReadHandler[max];

            // 常用类型绑定
            RegisterScalar(BuiltInType.Null, (d, f) => d.ReadVariant(f).Value);
            RegisterScalar(BuiltInType.Boolean, (d, f) => d.ReadBoolean(f));
            RegisterScalar(BuiltInType.SByte, (d, f) => d.ReadSByte(f));
            RegisterScalar(BuiltInType.Byte, (d, f) => d.ReadByte(f));
            RegisterScalar(BuiltInType.Int16, (d, f) => d.ReadInt16(f));
            RegisterScalar(BuiltInType.UInt16, (d, f) => d.ReadUInt16(f));
            RegisterScalar(BuiltInType.Int32, (d, f) => d.ReadInt32(f));
            RegisterScalar(BuiltInType.UInt32, (d, f) => d.ReadUInt32(f));
            RegisterScalar(BuiltInType.Int64, (d, f) => d.ReadInt64(f));
            RegisterScalar(BuiltInType.UInt64, (d, f) => d.ReadUInt64(f));
            RegisterScalar(BuiltInType.Float, (d, f) => d.ReadFloat(f));
            RegisterScalar(BuiltInType.Double, (d, f) => d.ReadDouble(f));
            RegisterScalar(BuiltInType.Number, (d, f) => d.ReadDouble(f));
            RegisterScalar(BuiltInType.String, (d, f) =>
            {
                if (d.ReadField(f, out var v))
                    return v is string ? v : v?.ToString();
                return null;
            });
            RegisterScalar(BuiltInType.DateTime, (d, f) => d.ReadDateTime(f));
            RegisterScalar(BuiltInType.Guid, (d, f) => d.ReadGuid(f));
            RegisterScalar(BuiltInType.ByteString, (d, f) => d.ReadByteString(f));
            RegisterScalar(BuiltInType.XmlElement, (d, f) => d.ReadXmlElement(f));
            RegisterScalar(BuiltInType.NodeId, (d, f) => d.ReadNodeId(f));
            RegisterScalar(BuiltInType.ExpandedNodeId, (d, f) => d.ReadExpandedNodeId(f));
            RegisterScalar(BuiltInType.StatusCode, (d, f) => d.ReadStatusCode(f));
            RegisterScalar(BuiltInType.QualifiedName, (d, f) => d.ReadQualifiedName(f));
            RegisterScalar(BuiltInType.LocalizedText, (d, f) => d.ReadLocalizedText(f));
            RegisterScalar(BuiltInType.ExtensionObject, (d, f) => d.ReadExtensionObject(f));
            RegisterScalar(BuiltInType.DataValue, (d, f) => d.ReadDataValue(f));
            RegisterScalar(BuiltInType.Enumeration, (d, f) =>
            {
                try
                {
                    var type = TypeInfo.GetSystemType(BuiltInType.Enumeration, ValueRanks.Scalar);
                    if (type != null && type.IsEnum)
                    {
                        return d.ReadEnumerated(f, type);
                    }
                }
                catch { }
                return d.ReadInt32(f);
            });
            RegisterScalar(BuiltInType.DiagnosticInfo, (d, f) => d.ReadDiagnosticInfo(f));
            RegisterScalar(BuiltInType.Variant, (d, f) => d.ReadVariant(f));
        }

        private static void RegisterScalar(BuiltInType type, ReadHandler handler)
        {
            var idx = (int)type;
            if (idx >= 0 && idx < _scalarHandlers.Length)
            {
                _scalarHandlers[idx] = handler;
            }
        }


        #endregion

        #region JToken Helpers (fast)

        internal static int CalculateActualValueRank(this JToken jToken)
        {
            if (jToken == null || jToken.Type != JTokenType.Array)
                return -1;

            int numDimensions = 0;
            var current = jToken;
            while (current is JArray arr)
            {
                numDimensions++;
                if (arr.Count == 0) break;
                current = arr[0];
            }
            return numDimensions;
        }

        private static bool ElementsHasSameType(this JToken[] jTokens)
        {
            if (jTokens == null || jTokens.Length == 0) return true;
            var first = jTokens[0].Type == JTokenType.Integer ? JTokenType.Float : jTokens[0].Type;
            for (int i = 1; i < jTokens.Length; i++)
            {
                var t = jTokens[i].Type == JTokenType.Integer ? JTokenType.Float : jTokens[i].Type;
                if (t != first) return false;
            }
            return true;
        }

        private static JTokenType GetElementsType(this JToken[] jTokens)
        {
            if (!jTokens.ElementsHasSameType())
                throw new Exception("The array sent must have the same type of element in each dimension");
            return jTokens.Length == 0 ? JTokenType.None : jTokens[0].Type;
        }

        internal static Type GetSystemType(JTokenType jsonType)
        {
            return jsonType switch
            {
                JTokenType.None => typeof(string),
                JTokenType.Object => typeof(string),
                JTokenType.Array => typeof(Array),
                JTokenType.Constructor => typeof(string),
                JTokenType.Property => typeof(string),
                JTokenType.Comment => typeof(string),
                JTokenType.Integer => typeof(long),
                JTokenType.Float => typeof(float),
                JTokenType.String => typeof(string),
                JTokenType.Boolean => typeof(bool),
                JTokenType.Null => typeof(string),
                JTokenType.Undefined => typeof(string),
                JTokenType.Date => typeof(DateTime),
                JTokenType.Raw => typeof(string),
                JTokenType.Bytes => typeof(byte[]),
                JTokenType.Guid => typeof(Guid),
                JTokenType.Uri => typeof(Uri),
                JTokenType.TimeSpan => typeof(TimeSpan),
                _ => typeof(string),
            };
        }

        #endregion


        #region StringBuilder Pool

        private static readonly ConcurrentBag<StringBuilder> _sbPool = new ConcurrentBag<StringBuilder>();
        private static StringBuilder RentStringBuilder()
        {
            if (_sbPool.TryTake(out var sb))
            {
                sb.Clear();
                return sb;
            }
            return new StringBuilder(1024);
        }
        private static void ReturnStringBuilder(StringBuilder sb)
        {
            if (sb.Capacity > 1024)
            {
                return;
            }
            _sbPool.Add(sb);
        }

        #endregion
    }
}