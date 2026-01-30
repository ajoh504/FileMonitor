using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FileMonitor.Udp
{
    public class UdpMessage
    {
        /// <summary>
        /// Allows the UdpClient to be passed into the AsyncCallback
        /// </summary>
        public struct UdpState
        {
            public UdpClient udpClient;
            public IPEndPoint ip;
        }

        public static bool MessageReceived { get; set; }

        public static void Callback(IAsyncResult asyncResult)
        {
            var udpClient = ((UdpState)asyncResult.AsyncState).udpClient;
            var ip = ((UdpState)asyncResult.AsyncState).ip;

            byte[] receiveBytes = udpClient.EndReceive(asyncResult, ref ip);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);
            int changedFileCount = Convert.ToInt32(receiveString);

            MessageReceived = true;
        }

        public void Receive(string host = "127.0.0.1", int portNumber = 9713)
        {
            var ip = new IPEndPoint(IPAddress.Parse(host), portNumber);
            var udpClient = new UdpClient();
            udpClient.Connect(ip);

            var state = new UdpState();
            state.udpClient = udpClient;
            state.ip = ip;  

            udpClient.BeginReceive(new AsyncCallback(Callback), state.udpClient);

            while (!MessageReceived)
            {
                Thread.Sleep(100);
            }
        }
    }
}
