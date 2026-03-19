using System.IO.Pipelines;
using System.Xml;
using TouchSocket.Core;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortReadTest_T
    {
        internal static async Task Run()
        {

            var doc = new XmlDocument();
            doc.Load("appsettings.xml");

            string? portname = doc.SelectSingleNode("/settings/portname")?.InnerText;
            var config = new TouchSocket.Core.TouchSocketConfig();
            config.SetSerialPortOption(options =>
            {
                options.BaudRate = 115200;//波特率
                options.DataBits = 8;//数据位
                options.Parity = System.IO.Ports.Parity.None;//校验位
                options.PortName = portname;//COM
                options.StopBits = System.IO.Ports.StopBits.One;//停止位
                //options.PollingDelay = 1;//轮询间隔
                options.StreamAsync = false;//同步模式
                options.ReadBufferSize = 1024 * 100;
                options.WriteBufferSize = 1024 * 100;
            });
            config.SetTransportOption(options =>
            {
                options.BufferOnDemand = false;

                options.ReceivePipeOptions = new PipeOptions(
              readerScheduler: PipeScheduler.ThreadPool,
              writerScheduler: PipeScheduler.ThreadPool,
              pauseWriterThreshold: 2 * 1024 * 1024,
              resumeWriterThreshold: 1024 * 1024,
              minimumSegmentSize: 8192,
              useSynchronizationContext: false);

                options.SendPipeOptions = new PipeOptions(
              readerScheduler: PipeScheduler.ThreadPool,
              writerScheduler: PipeScheduler.ThreadPool,
              pauseWriterThreshold: 128 * 1024,
              resumeWriterThreshold: 64 * 1024,
              minimumSegmentSize: 8192,
              useSynchronizationContext: false);
            });


            config.SetSerialDataHandlingAdapter(() => new FixedSizePackageAdapter(13));
            var client = new SerialPortClient();
            client.Received = ChannelReceived;
            await client.SetupAsync(config).ConfigureAwait(false);

            await client.ConnectAsync().ConfigureAwait(false);
            _ = Task.Run(async () =>
             {
                 while (true)
                 {
                     try
                     {
                         await client.ConnectAsync().ConfigureAwait(false);

                     }
                     catch (Exception ex)
                     {
                         Console.WriteLine(ex);
                     }
                 }
             });
        }


        private static int index = 1;
        private static DateTime start;
        private static DateTime end;
        private static async Task ChannelReceived(ISerialPortClient channel, ReceivedDataEventArgs args)
        {

            var data = args.Memory;
            end = DateTime.Now;
            if (index > 1)
            {
                await Console.Out.WriteLineAsync(DateTime.Now.ToString("HH:mm:ss:ffff") + "--接收序号" + (index) + "--数据长度" + data.Length + "--MS: " + (end - start).TotalMilliseconds).ConfigureAwait(false);
            }
            index += 1;
            start = DateTime.Now;

        }
    }
}
