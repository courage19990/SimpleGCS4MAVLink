using System.ComponentModel;

namespace SimpleGCS4MAVLink
{
    // 一个GCS对应一个飞控，以及一个后台线程 
    class GCS4MAVLink
    {   
        // GCS的GUI层需要实现这个方法，并将该方法的实例挂载到GCS的相应委托变量，以便通过回调实时更新GUI组件状态
        public delegate void MessageHandler(MAVLink.MAVLinkMessage packet);

        // 当接收到数据时进行回调的方法，在创建GCS实例时，必须提供至少一个消息处理器，后续可以继续挂载
        public MessageHandler messageHandler;

        // 一个GCS对应一个飞控
        private Autopilot autopilot = new Autopilot();

        // 飞控和GCS具有完全一致的MAVLink数据结构
        private MAVLink.MavlinkParse mavlink = new MAVLink.MavlinkParse();
        
        // 防止多线程重复读，留待后续备用
        private object readlock = new object();
        
        // 对应飞控的系统id，保留备用
        private byte sysid;
        
        // 对应飞控的组件id，保留备用
        private byte compid;

        public void Startup(string portName, int baudRate, MessageHandler messageHandler)
        {
            if (autopilot.IsConnected()) return;
            // 连接飞控
            autopilot.Connect(portName, baudRate);
            // 挂载消息处理器
            this.messageHandler = messageHandler;
            // 运行后台线程
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += Jobs;
            bgw.RunWorkerAsync();
        }

        public void Shutdown()
        {
            if (autopilot.IsConnected())
            {
                autopilot.CloseConnection();
            }                
        }

        public bool IsStartup()
        {
            return autopilot.IsConnected();
        }

        // 该线程的工作为：接收消息并传递给回调函数，返回心跳包
        private void Jobs(object sender, DoWorkEventArgs e)
        {
            while (autopilot.IsConnected())
            {
                try
                {
                    MAVLink.MAVLinkMessage packet;
                    lock (readlock)
                    {
                        // 从飞控读取有效的packet
                        packet = autopilot.GetPacket();
                        if (packet == null || packet.data == null)
                            continue; // 如果是无效的packet则直接进入下一次循环
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

                    // 检查消息是否来自相应飞控
                    if (sysid != packet.sysid || compid != packet.compid)
                        continue; // 不是就不处理

                    messageHandler(packet);
                }
                catch
                {
                    // try catch是必要的，否则会因为抛出无关紧要的异常而结束线程。
                } 

                System.Threading.Thread.Sleep(1); // 防止单个线程将CPU占满？
            }
        }
    }
}
