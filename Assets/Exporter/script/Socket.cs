using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;  //套接字的命名空间
using System.Net; //IPAddress的命名空间
using System.Threading; //线程的命名空间

namespace EasySocket
{
    class Connector
    {
        public struct SocketSource : DataSource
        {
            Socket socket;
            public SocketSource(Socket socket)
            {
                this.socket = socket;
            }
            public int Read(byte[] buffer, int start, int length)
            {
                return socket.Receive(buffer, start, length, SocketFlags.None);
            }
            public int Write(byte[] buffer, int start, int length)
            {
                return socket.Send(buffer, start, length, SocketFlags.None);
            }
        }
        Socket socket;
        CircularBuffer receive;
        CircularBuffer send;
        public Connector(Socket socket, int receiveBufferSize, int senBufferSize)
        {
            this.socket = socket;
            this.receive = new CircularBuffer(receiveBufferSize);
            this.send = new CircularBuffer(senBufferSize);
        }

        public void UpdateBuffer()
        {
            var source = new SocketSource(socket);
            receive.WriteFromSource(source);
            send.ReadToSource(source);
        }
        public void Close()
        {
            socket.Close();
        }
    }
    class Server
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ip;
        int port;
        IPEndPoint ipEndPoint;
        public Server(string ip, int port)
        {
            var _ip = IPAddress.Parse(ip);
            this.ip = _ip;
            this.port = port;
            this.ipEndPoint = new IPEndPoint(_ip, port);
            socket.Bind(ipEndPoint);
        }
        public int maxPendingClients = 1;
        public int maxClients = 1;
        public int connectorReceiveBufferSize = 1024;
        public int connectorSendBufferSize = 1024;
        List<Connector> clients = new List<Connector>();
        void FindClient()
        {
            socket.Listen(maxPendingClients);
            while (clients.Count < maxClients)
            {
                var newClient = new Connector(socket.Accept(), connectorReceiveBufferSize, connectorSendBufferSize);
                lock (clients)
                {
                    clients.Add(newClient);
                }
            }
        }
        void UpdateConnectorBuffer()
        {
            lock (clients)
            {
                while (true)
                {
                    foreach (var connector in clients)
                    {
                        connector.UpdateBuffer();
                    }
                }
            }
        }
        Thread findClient;
        Thread updateConnectorBuffer;
        bool created = false;
        public void Create()
        {
            if (created) { throw new Exception(); }
            else { created = true; }
            findClient = new Thread(FindClient);
            findClient.Start();
            updateConnectorBuffer = new Thread(UpdateConnectorBuffer);
            updateConnectorBuffer.Start();
        }
        public void Release()
        {
            if (!created) { throw new Exception(); }
            else { created = false; }
            findClient.Abort();
            updateConnectorBuffer.Abort();
            foreach (var connector in clients)
            {
                connector.Close();
            }
            clients.Clear();
            socket.Close();
        }
        ~Server()
        {
            if (created)
            {
                Release();
            }
        }
    }
}
