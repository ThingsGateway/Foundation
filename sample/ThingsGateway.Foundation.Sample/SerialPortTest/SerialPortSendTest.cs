using System.IO.Ports;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortSendTest
    {
        internal static async Task Run()
        {

            _ = Task.Run(async () =>
            {

                SerialPort serialPort = new();
                serialPort.PortName = "COM4";
                serialPort.BaudRate = 115200;//波特率
                serialPort.DataBits = 8;//波特率
                serialPort.Parity = System.IO.Ports.Parity.None;//校验位
                serialPort.StopBits = System.IO.Ports.StopBits.One;//停止位
                serialPort.ReadBufferSize = 1024 * 100;
                serialPort.WriteBufferSize = 1024 * 100;
                serialPort.Open();
                while (true)
                {
                    serialPort.Write(new byte[13], 0, 13);
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
);
        }

    }
}
