using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasySocket;

namespace Exporter
{
    public class MainContext : MonoBehaviour
    {
        [SerializeField] string ip;
        [SerializeField] int port;
        Server server;
        [ContextMenu("CreateServer")]
        void CreateServer()
        {
            server = new Server(ip, port);
            server.Create();
        }
        [ContextMenu("ReleaseServer")]
        void ReleaseServer()
        {
            server.Release();
        }
    }
}