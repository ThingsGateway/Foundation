using ThingsGateway.Foundation.Common.Json.Extension;

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

    }
}
