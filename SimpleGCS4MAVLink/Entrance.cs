using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGCS4MAVLink
{
    public partial class Entrance : Form
    {
        MAVLink.MavlinkParse mavlink = new MAVLink.MavlinkParse();
        // locking to prevent multiple reads on serial port
        object readlock = new object();
        // our target sysid
        byte sysid;
        // our target compid
        byte compid;

        // 简易的地面站，暂时只支持与一架无人机通信
        Autopilot autopilot = new Autopilot();

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
            // 每个GCS服务一个飞控
            // GCS[i].starup()
            // GCS.ConncetToAutopilot( com,  baudrate)
            // GCS.返回心跳包
            // GCS.sendREQUESTpacket
            // GCS.回调函数（show_gps_info）
            // void show_gps_info( da ta )
            
            if (autopilot.IsConnected())
            {
                autopilot.Close();
                return;
            }

            autopilot.Connect(cmbComport.Text, int.Parse(cmbBaudrate.Text));

            BackgroundWorker bgw = new BackgroundWorker();

            bgw.DoWork += bgw_DoWork;

            bgw.RunWorkerAsync();
        }

        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (autopilot.IsConnected())
            {
                try
                {
                    MAVLink.MAVLinkMessage packet;
                    lock (readlock)
                    {
                        // read any valid packet from the port
                        packet = autopilot.GetPacket();
                        // check its valid
                        if (packet == null || packet.data == null)
                            continue;
                    }
                    // check to see if its a hb packet from the comport
                    if (packet.data.GetType() == typeof(MAVLink.mavlink_heartbeat_t))
                    {
                        var hb = (MAVLink.mavlink_heartbeat_t)packet.data;

                        // save the sysid and compid of the seen MAV
                        sysid = packet.sysid;
                        compid = packet.compid;

                        // request streams at 2 hz
                        var buffer = mavlink.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.REQUEST_DATA_STREAM,
                            new MAVLink.mavlink_request_data_stream_t()
                            {
                                req_message_rate = 2,
                                req_stream_id = (byte)MAVLink.MAV_DATA_STREAM.ALL,
                                start_stop = 1,
                                target_component = compid,
                                target_system = sysid
                            });

                        autopilot.ReceiveBytes(buffer);

                        buffer = mavlink.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.HEARTBEAT, hb);

                        autopilot.ReceiveBytes(buffer);
                    }

                    // from here we should check the the message is addressed to us
                    if (sysid != packet.sysid || compid != packet.compid)
                        continue;

                    Console.WriteLine(packet.msgtypename);

                    if (packet.msgid == (byte)MAVLink.MAVLINK_MSG_ID.ATTITUDE)
                    //or
                    //if (packet.data.GetType() == typeof(MAVLink.mavlink_attitude_t))
                    {
                        var att = (MAVLink.mavlink_attitude_t)packet.data;

                        Console.WriteLine(att.pitch * 57.2958 + " " + att.roll * 57.2958);
                    }
                }
                catch
                {
                }

                System.Threading.Thread.Sleep(1);
            }
        }
    }
}
