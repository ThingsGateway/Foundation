using System.Text.Json.Nodes;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.Extension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class JsonHelperTests
    {
        [Fact]
        public void ToSystemTextJsonStringNumber()
        {
            ushort value = 0;
            var data = value.ToSystemTextJsonString(false);
            Assert.Equal("0", data);
        }

        [Fact]
        public void GetObjectFromJsonNode_Should_Work_For_All_Primitive_Types()
        {
            // null
            {
                var v = JsonValue.Create((string?)null);
                var r = JsonUtil.GetObjectFromJsonNode(v);
                Assert.Null(r);
            }

            // string
            {
                var v = JsonValue.Create("hello");
                var r = JsonUtil.GetObjectFromJsonNode(v);
                Assert.Equal("hello", r);
            }

            // DateTime (string -> DateTime)
            {
                var dt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);
                var v = JsonValue.Create(dt.ToString("O"));
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<DateTime>(r);
                Assert.Equal(dt, (DateTime)r);
            }

            // Guid (string -> Guid)
            {
                var g = Guid.NewGuid();
                var v = JsonValue.Create(g.ToString());
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<Guid>(r);
                Assert.Equal(g, (Guid)r);
            }

            // bool
            {
                var v = JsonValue.Create(true);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<bool>(r);
                Assert.Equal(true, r);
            }

            // int
            {
                var v = JsonValue.Create(123);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<int>(r);
                Assert.Equal(123, r);
            }

            // long
            {
                long value = (long)int.MaxValue + 10;
                var v = JsonValue.Create(value);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<long>(r);
                Assert.Equal(value, r);
            }

            // double
            {
                var v = JsonValue.Create(1.25d);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsType<double>(r);
                Assert.Equal(1.25d, (double)r, 0.0000001);
            }
        }
    }
}
