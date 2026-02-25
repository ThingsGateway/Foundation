using System.Text.Json.Nodes;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.Extension;

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    public class JsonHelperTests
    {


        [TestMethod]
        public void ToSystemTextJsonStringNumber()
        {
            ushort value = 0;
            var data = value.ToSystemTextJsonString(false);
            Assert.AreEqual("0", data);
        }


        [TestMethod]
        public void GetObjectFromJsonNode_Should_Work_For_All_Primitive_Types()
        {
            // null
            {
                var v = JsonValue.Create((string?)null);
                var r = JsonUtil.GetObjectFromJsonNode(v);
                Assert.IsNull(r);
            }

            // string
            {
                var v = JsonValue.Create("hello");
                var r = JsonUtil.GetObjectFromJsonNode(v);
                Assert.AreEqual("hello", r);
            }

            // DateTime (string -> DateTime)
            {
                var dt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);
                var v = JsonValue.Create(dt.ToString("O"));
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(DateTime));
                Assert.AreEqual(dt, (DateTime)r);
            }

            // Guid (string -> Guid)
            {
                var g = Guid.NewGuid();
                var v = JsonValue.Create(g.ToString());
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(Guid));
                Assert.AreEqual(g, (Guid)r);
            }

            // bool
            {
                var v = JsonValue.Create(true);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(bool));
                Assert.AreEqual(true, r);
            }

            // int
            {
                var v = JsonValue.Create(123);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(int));
                Assert.AreEqual(123, r);
            }

            // long
            {
                long value = (long)int.MaxValue + 10;
                var v = JsonValue.Create(value);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(long));
                Assert.AreEqual(value, r);
            }

            // double
            {
                var v = JsonValue.Create(1.25d);
                var r = JsonUtil.GetObjectFromJsonNode(v);

                Assert.IsInstanceOfType(r, typeof(double));
                Assert.AreEqual(1.25d, (double)r, 0.0000001);
            }
        }
        }
}
