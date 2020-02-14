using Newtonsoft.Json;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SimpleGCS4MAVLink
{
    // 作为地面站程序的入口
    public partial class Entrance : Form
    {
        private GCS4MAVLink gcs = new GCS4MAVLink();

        // 定义一个更新GUI的委托类，暂时弃用
        public delegate void UIUpdator(MAVLink.MAVLinkMessage packet);
        // 定义一个委托变量的引用，暂时弃用
        private UIUpdator updator;

        // 将数据通过websocket发送到浏览器显示，目前采用的方案
        private MyWebSocketServer server = new MyWebSocketServer("ws://127.0.0.1:8080");

        public Entrance()
        {
            InitializeComponent();
            updator = MyUIUpdator; // 暂时弃用
        }

        private void cmbComport_Click(object sender, EventArgs e)
        {
            cmbComport.DataSource = SerialPort.GetPortNames();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (gcs.IsStartup())
            {
                gcs.Shutdown();
                return;
            }
            gcs.Startup(cmbComport.Text, int.Parse(cmbBaudrate.Text), MessageHandler);
        }

        // 该方法在GCS开启的子线程中执行
        private void MessageHandler(MAVLink.MAVLinkMessage packet)
        {
            // Console.WriteLine(packet.msgtypename + " ##" + packet.msgid); 

            // BeginInvoke(updator, packet); 若要实现C#的GUI版本，取消该行注释

            server.SendMessage(JsonConvert.SerializeObject(packet));
        }

        // 该方法在主线程中执行
        private void MyUIUpdator(MAVLink.MAVLinkMessage packet)
        {
            // 若要实现C#的GUI版本，在这里更新UI组件
        }
    }
}
