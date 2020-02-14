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
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
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
