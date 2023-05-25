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
        [SerializeField] Protocol protocol;
        [ContextMenu("CreateServer")]
        void CreateNode()
        {
            node = new NetNode(ip, port, protocol);
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
        [ContextMenu("Send")]
        void Send()
        {
            node.connectorList[0].SendAction(new PrintInt(114514));
        }
        [ContextMenu("Execute")]
        void UpdateData()
        {
            node.ExcecuteAction();
        }
    }
}