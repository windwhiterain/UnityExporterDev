using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exporter
{
    using EasySocket;
    public class MainContext : MonoBehaviour
    {
        [SerializeField] string ip;
        [SerializeField] int port;
        NetNode node;
        [ContextMenu("CreateServer")]
        void CreateNode()
        {
            node = new NetNode(ip, port);
            node.Create();
        }
        [ContextMenu("ReleaseServer")]
        void ReleaseNode()
        {
            node.Release();
        }
        [SerializeField] string connectIp;
        [SerializeField] int connectPort;
        [ContextMenu("NodeConnect")]
        void NodeConnect()
        {
            node.Connect(connectIp, connectPort);
        }
        [ContextMenu("Test")]
        void Test()
        {
            node.connectorList[0].Send(new SmallData<System.Int32>(114514));
        }
    }
}