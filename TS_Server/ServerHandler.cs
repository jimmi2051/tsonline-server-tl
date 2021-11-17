using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TS_Server.Client;

namespace TS_Server
{
    class ServerHandler
    { 
        public AsyncCallback skCallBack;
        public Socket m_mainSocket;
        public Hashtable socketList = new Hashtable();
        public int m_clientCount = 0;

        public ServerHandler(int port)
        {
            try
            {
                m_mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port);
                m_mainSocket.Bind(ipLocal);
                m_mainSocket.Listen(4);
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                Socket workerSocket = m_mainSocket.EndAccept(asyn);
                Interlocked.Increment(ref m_clientCount);

                string clientID = System.Guid.NewGuid().ToString("N").Substring(0, 12);
                TSClient client = new TSClient(workerSocket, clientID);
                socketList.Add(clientID, workerSocket);

                // Session
                skCallBack = new AsyncCallback(OnDataReceived);
                WaitForData(workerSocket, client);

                Console.WriteLine("Client " + clientID + " is connected.");
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("OnClientConnection: Socket has been closed");
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void WaitForData(Socket soc, TSClient client)
        {
            try
            {
                PacketProcessor theSocPkt = new PacketProcessor(soc, client);

                soc.BeginReceive(theSocPkt.DataBuff, 0,
                    theSocPkt.DataBuff.Length,
                    SocketFlags.None,
                    skCallBack,
                    theSocPkt);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            PacketProcessor packet = (PacketProcessor)asyn.AsyncState;

            try
            {
                if (packet.gatherPacket() == 0)
                    WaitForData(packet.socket, packet.client);
                else
                {
                    Console.WriteLine("Client " + packet.client.getClientID() + " disconnected");
                    packet.client.disconnect();
                    socketList.Remove(packet.client.getClientID());
                    packet.socket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Client " + packet.client.getClientID() + " disconnected due to exception");
                packet.client.disconnect();
                socketList.Remove(packet.client.getClientID());
                packet.socket.Close();                
            }
        }

    }
}
