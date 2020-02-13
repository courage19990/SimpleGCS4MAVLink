using System.IO.Ports;

namespace SimpleGCS4MAVLink
{
    // 采用MAVLink作为通信协议的飞控
    class Autopilot
    {
        // 本质是作为serialPort的代理人
        private SerialPort serialPort = new SerialPort();
        // 飞控和GCS具有完全一致的MAVLink数据结构
        private MAVLink.MavlinkParse mavlink = new MAVLink.MavlinkParse();

        public void Connect(string portName, int baudRate)
        {
            serialPort.PortName = portName;
            serialPort.BaudRate = baudRate;
            serialPort.Open();
            serialPort.ReadTimeout = 2000;
        }

        public void CloseConnection()
        {
            serialPort.Close();
        }

        public bool IsConnected()
        {
            return serialPort.IsOpen;
        }

        public MAVLink.MAVLinkMessage GetPacket()
        {
            return mavlink.ReadPacket(serialPort.BaseStream);
        }

        public void ReceiveBytes(byte[] bytes) 
        {
            serialPort.Write(bytes, 0, bytes.Length);
        }
    }
}
