using System.IO.Ports;

namespace ThingsGateway.Foundation.Sample
{
#pragma warning disable CA2000 // 丢失范围之前释放对象
    internal sealed class SerialPortTest
    {
        internal static void Run()
        {
            SerialPort serialPort = new();
            serialPort.PortName = "COM2";
            serialPort.Open();
            while (true)
            {
                serialPort.Write(new byte[150], 0, 150);
            }
        }
    }
}
