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
        [ContextMenu("Send")]
        void Send()
        {
            node.connectorList[0].Send(new SmallData<System.Single>(1.2345f));
        }
        SmallData<System.Single> testData = new SmallData<System.Single>();
        [ContextMenu("Receive")]
        void Receive()
        {
            node.connectorList[0].Receive(testData);
        }
        [ContextMenu("Result")]
        void Result()
        {
            if (testData.Complete)
            {
                Debug.Log(testData.Uncoded);
            }
        }
        [ContextMenu("UpdateData")]
        void UpdateData()
        {
            node.UpdateData();
        }
    }
}