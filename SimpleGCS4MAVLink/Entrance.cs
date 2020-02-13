using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace SimpleGCS4MAVLink
{
    // 作为地面站程序的入口
    public partial class Entrance : Form
    {
        private GCS4MAVLink gcs = new GCS4MAVLink();

        public Entrance()
        {
            InitializeComponent();
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

        private void MessageHandler(MAVLink.MAVLinkMessage packet)
        {
            Console.WriteLine(packet.msgtypename);
        }
    }
}
