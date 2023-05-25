using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;  //套接字的命名空间
using System.Net; //IPAddress的命名空间
using System.Threading; //线程的命名空间

namespace Exporter
{
    namespace EasySocket
    {
        public class Connector
        {
            public struct SocketSource : DataSource
            {
                Socket socket;
                public SocketSource(Socket socket)
                {
                    this.socket = socket;
                }
                public int ReadTo(byte[] buffer, int start, int length)
                {
                    return socket.Receive(buffer, start, length, SocketFlags.None);
                }
                public int WriteFrom(byte[] buffer, int start, int length)
                {
                    return socket.Send(buffer, start, length, SocketFlags.None);
                }
            }
            Socket socket;
            CircularBuffer receive;
            CircularBuffer send;
            public Connector(Socket socket)
            {
                this.socket = socket;
            }
            public DataSource Source
            {
                get { return new SocketSource(socket); }
            }
            DataSourcer toSend = new DataSourcer(true);
            DataSourcer toReceive = new DataSourcer(false);
            public void Send(Data data)
            {
                toSend.Add(data);
            }
            public void Receive(Data data)
            {
                toReceive.Add(data);
            }
            class DataSourcer
            {
                bool readOrWrite;
                public DataSourcer(bool readOrWrite)
                {
                    this.readOrWrite = readOrWrite;
                }
                Queue<Data> pending = new Queue<Data>();
                Data onSourcing;
                public void Add(Data data) { pending.Enqueue(data); }
                bool NextData()
                {
                    if (pending.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        onSourcing = pending.Dequeue();
                        return true;
                    }
                }
                bool UnCompleteData()
                {
                    while (true)
                    {
                        if (onSourcing == null || onSourcing.Complete)
                        {
                            if (!NextData()) { return false; }
                        }
                        else
                        {
                            return true;
                        }
                        Debug.Log(1);
                    }
                }
                public void Update(DataSource source)
                {
                    while (true)
                    {
                        if (UnCompleteData())
                        {
                            if (readOrWrite)
                            {
                                onSourcing.ReadTo(source);
                            }
                            else
                            {
                                onSourcing.WriteFrom(source);
                            }
                            if (!onSourcing.Complete) { break; }
                        }
                        else
                        {
                            break;
                        }
                        Debug.Log(2);
                    }
                }
            }
            public void UpdateData()
            {
                toSend.Update(Source);
                toReceive.Update(Source);
            }
            public void Close()
            {
                socket.Close();
            }
        }
        class NetNode
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip;
            int port;
            IPEndPoint ipEndPoint;
            public NetNode(string ip, int port)
            {
                var _ip = IPAddress.Parse(ip);
                this.ip = _ip;
                this.port = port;
                this.ipEndPoint = new IPEndPoint(_ip, port);
                socket.Bind(ipEndPoint);
            }
            public int maxPendingClients = 1;
            public int maxClients = 1;
            public List<Connector> connectorList = new List<Connector>();
            void FindClient()
            {
                socket.Listen(maxPendingClients);
                while (connectorList.Count < maxClients)
                {
                    var newSocket = new Connector(socket.Accept());
                    lock (connectorList)
                    {
                        connectorList.Add(newSocket);
                    }
                }
            }
            struct ConnectJob
            {
                string ip;
                int port;
                NetNode node;
                public ConnectJob(string ip, int port, NetNode node)
                {
                    this.ip = ip;
                    this.port = port;
                    this.node = node;
                    var thread = new Thread(Connect);
                    thread.Start();
                }
                void Connect()
                {
                    var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    newSocket.Connect(IPAddress.Parse(ip), port);
                    node.connectorList.Add(new Connector(newSocket));
                }
            }

            public void Connect(string ip, int port)
            {
                if (!created) { throw new Exception(); }
                new ConnectJob(ip, port, this);
            }
            public void UpdateData()
            {
                foreach (var connector in connectorList)
                {
                    connector.UpdateData();
                }
            }
            Thread findClient;
            bool created = false;
            public void Create()
            {
                if (created) { throw new Exception(); }
                else { created = true; }
                findClient = new Thread(FindClient);
                findClient.Start();
            }
            public void Release()
            {
                if (!created) { throw new Exception(); }
                else { created = false; }
                findClient.Abort();
                foreach (var connector in connectorList)
                {
                    connector.Close();
                }
                connectorList.Clear();
                socket.Close();
            }
            ~NetNode()
            {
                if (created)
                {
                    Release();
                }
            }
        }
    }
}