using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Exporter
{
    using i32 = SmallData<System.Int32>;
    using f64 = SmallData<System.Single>;
    using ni32 = ArrayData<System.Int32>;
    public class Protocol : MonoBehaviour
    {
        [System.Serializable]
        public class IdNamePair { public int id; public string typeName; }
        [System.Serializable]
        public class ProtocolStruct { public IdNamePair[] pairs; }
        [SerializeField] ProtocolStruct protocolStruct;
        [SerializeField] string path;
        [ContextMenu("Parse")]
        public void Parse()
        {
            string json = File.ReadAllText(path);
            protocolStruct = JsonUtility.FromJson<ProtocolStruct>(json);
            var nameIdMap = new Dictionary<string, int>();
            foreach (var pair in protocolStruct.pairs)
            {
                nameIdMap.Add(pair.typeName, pair.id);
            }
            idActionMap = new CreateAction[protocolStruct.pairs.Length];
            for (int i = 0; i < implementedAction.Length; i++)
            {
                var creator = implementedAction[i];
                var name = creator().name;
                if (nameIdMap.ContainsKey(name))
                {
                    var id = nameIdMap[name];
                    idActionMap[id] = creator;
                }
            }
            for (int i = 0; i < idActionMap.Length; i++)
            {
                if (idActionMap[i] == null)
                {
                    throw new System.Exception("当前版本未实现协议中的类型:" + i);
                }
            }
        }
        delegate Action CreateAction();
        CreateAction[] implementedAction = new CreateAction[]{
            ()=>new PrintInt()
        };
        CreateAction[] idActionMap;
        public Action ChooseResponse(int id)
        {
            return idActionMap[id]();
        }
    }
}