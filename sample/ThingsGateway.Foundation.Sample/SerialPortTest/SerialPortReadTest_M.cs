using System.IO.Ports;
using System.Xml;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortReadTest_M
    {
        internal static void Run()
        {
            var doc = new XmlDocument();
            doc.Load("appsettings.xml");

            string? portname = doc.SelectSingleNode("/settings/portname")?.InnerText;
            SerialPort serialPort1 = new();
            serialPort1.PortName = portname;
            serialPort1.BaudRate = 115200;//波特率
            serialPort1.DataBits = 8;//波特率
            serialPort1.Parity = System.IO.Ports.Parity.None;//校验位
            serialPort1.StopBits = System.IO.Ports.StopBits.One;//停止位
            serialPort1.ReadBufferSize = 1024 * 100;
            serialPort1.WriteBufferSize = 1024 * 100;
            serialPort1.Open();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {

                        //if (serialPort1.BytesToRead >= 13)
                        {
                            // 读取当前接收缓冲区的所有数据
                            end = DateTime.Now;
                            byte[] receivedBytes = new byte[13];
                            var count = serialPort1.Read(receivedBytes, 0, 13);
                            if (index > 1)
                                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + "--接收序号" + (index) + "--数据长度" + count + "--MS: " + (end - start).TotalMilliseconds);
                            index += 1;
                            start = DateTime.Now;
                        }
                        //else
                        {
                            await Task.Delay(1).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await Task.Delay(1).ConfigureAwait(false);
                    }
                }
            });


        }


        private static int index = 1;
        private static DateTime start;
        private static DateTime end;
    }
}
