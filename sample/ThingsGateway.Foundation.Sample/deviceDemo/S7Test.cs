using ThingsGateway.Foundation.SiemensS7;
#pragma warning disable CA2000 // 丢失范围之前释放对象

namespace ThingsGateway.Foundation.Sample
{
    internal class S7Test
    {

        /// <summary>
        /// 新建协议对象
        /// </summary>
        /// <returns></returns>
        public SiemensS7Master GetDevice()
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var client = new SiemensS7Master();
            var clientChannel = client.CreateChannel(clientConfig, new ChannelOptions() { ChannelType = ChannelTypeEnum.TcpClient, RemoteUrl = "127.0.0.1:102" });
            client.InitChannel(new(clientChannel));
            client.SiemensS7Type = SiemensTypeEnum.S1500;
            return client;
        }


        public async Task Test()
        {
            var s7 = GetDevice();
            {
                var data = await s7.ReadAsync("DB1.1", 2).ConfigureAwait(false);
                if (data.IsSuccess)
                {
                    var uint16 = s7.BitConverter.ToUInt16(data.Content.Span, 0);//自行解析
                }

                var int32Result = await s7.ReadInt32Async("DB1.1", 1).ConfigureAwait(false);



            }
        }


    }
}
