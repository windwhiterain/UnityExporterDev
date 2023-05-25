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
                    if (socket.Available == 0) { return 0; }
                    return socket.Receive(buffer, start, Mathf.Min(length, socket.Available), SocketFlags.None);
                }
                public int WriteFrom(byte[] buffer, int start, int length)
                {
                    return socket.Send(buffer, start, length, SocketFlags.None);
                }
            }
            Socket socket;
            CircularBuffer receive;
            CircularBuffer send;
            public Connector(Socket socket, Protocol protocol)
            {
                this.socket = socket;
                this.protocol = protocol;
            }
            public DataSource Source
            {
                get { return new SocketSource(socket); }
            }
            DataSourcer toSend = new DataSourcer(true);
            DataSourcer toReceive = new DataSourcer(false);
            void SendData(Data data)
            {
                toSend.Add(data);
            }
            void ReceiveData(Data data)
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
                    }
                }
                public void Update(DataSource source)
                {
                    Debug.Log("Update(");
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
                    }
                    Debug.Log("Update)");
                }
            }
            void UpdateData()
            {
                Debug.Log("UpdateData(");
                toSend.Update(Source);
                toReceive.Update(Source);
                Debug.Log("UpdateData)");
            }
            public void SendAction(Action action)
            {
                SendData(new SmallData<Int32>(action.id));
                SendData(action.InputData);
            }
            Protocol protocol;
            Queue<Action> actionReadyQueue = new Queue<Action>();
            Action onReceive;
            SmallData<System.Int32> actionId;
            void CheckReady()
            {
                if (onReceive != null)
                {
                    if (onReceive.Ready)
                    {
                        lock (actionReadyQueue)
                        {
                            actionReadyQueue.Enqueue(onReceive);
                        }
                        onReceive = null;
                        Debug.Log("ActionReady");
                    }
                    else
                    {
                        return;
                    }
                }
            }
            void CheckNewAction()
            {
                if (actionId != null)
                {
                    if (actionId.Complete && onReceive == null)
                    {
                        onReceive = protocol.ChooseResponse(actionId.Uncoded);
                        ReceiveData(onReceive.InputData);
                        actionId = new SmallData<System.Int32>();
                        ReceiveData(actionId);
                        Debug.Log("RecvAction");
                    }
                }
                else
                {
                    actionId = new SmallData<System.Int32>();
                    ReceiveData(actionId);
                    Debug.Log("RecvIdData");
                }
            }
            void UpdateAction()
            {
                Debug.Log("UpdateAction(");
                CheckReady();
                CheckNewAction();
                Debug.Log("UpdateAction)");
            }
            void ExecuteAction()
            {

                if (actionReadyQueue.Count != 0)
                {
                    Action action;
                    lock (actionReadyQueue)
                    {
                        action = actionReadyQueue.Dequeue();
                    }
                    action.Excecute();
                }
            }
            public void Update()
            {
                Debug.Log("Update(");
                UpdateData();
                UpdateAction();
                ExecuteAction();
                Debug.Log("Update)");
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
            Protocol protocol;
            public NetNode(string ip, int port, Protocol protocol)
            {
                this.protocol = protocol;
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
                    var newSocket = new Connector(socket.Accept(), protocol);
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
                Protocol protocol;
                public ConnectJob(string ip, int port, NetNode node, Protocol protocol)
                {
                    this.ip = ip;
                    this.port = port;
                    this.node = node;
                    this.protocol = protocol;
                    var thread = new Thread(Connect);
                    thread.Start();
                }
                void Connect()
                {
                    var newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    newSocket.Connect(IPAddress.Parse(ip), port);
                    node.connectorList.Add(new Connector(newSocket, protocol));
                }
            }

            public void Connect(string ip, int port)
            {
                if (!created) { throw new Exception(); }
                new ConnectJob(ip, port, this, protocol);
            }
            public void Update()
            {
                foreach (var connector in connectorList)
                {
                    connector.Update();
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