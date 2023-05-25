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
            node.connectorList[0].Send(new ArrayData<System.Int32>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
        }
        ArrayData<System.Int32> testData = new ArrayData<System.Int32>();
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
                Debug.Log(testData.Uncoded.ToString());
            }
        }
        [ContextMenu("UpdateData")]
        void UpdateData()
        {
            node.UpdateData();
        }
    }
}