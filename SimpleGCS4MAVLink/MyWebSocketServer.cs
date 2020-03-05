using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGCS4MAVLink
{
    class MyWebSocketServer
    {
        private List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        private WebSocketServer server;

        public MyWebSocketServer(string location)
        {
            server = new WebSocketServer(location);
            server.Start(socket =>
            {
                string clientIpAddress = socket.ConnectionInfo.ClientIpAddress;
                int clientPort = socket.ConnectionInfo.ClientPort;
                socket.OnOpen = () =>
                {
                    Console.WriteLine("浏览器前端{0}:{1}已连接。", clientIpAddress, clientPort);
                    allSockets.Add(socket); // 同一PC也支持不同端口的多个web socket进行连接
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("浏览器前端{0}:{1}已断开。", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort);
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    // Console.WriteLine(message);
                    // 与浏览器前端的更多交互在这里编辑。
                };
            });
        }

        public void SendMessage(string message)
        {
            foreach (var socket in allSockets.ToList())
            {
                socket.Send(message);
            }
        }
    }
}
