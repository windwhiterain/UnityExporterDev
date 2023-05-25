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
        public class IdNameMap { public IdNamePair[] pairs; }
        [SerializeField] IdNameMap idNameMap;
        [SerializeField] string path;
        [ContextMenu("Parse")]
        public void Parse()
        {
            string json = File.ReadAllText(path);
            idNameMap = JsonUtility.FromJson<IdNameMap>(json);
            indexIdMap = new int[idNameMap.pairs.Length];
            foreach (var pair in idNameMap.pairs)
            {
                if (!nameIndexMap.ContainsKey(pair.typeName))
                {
                    Debug.Log("当前版本未实现协议中的类型:" + pair.typeName);
                    continue;
                }
                indexIdMap[nameIndexMap[pair.typeName]] = pair.id;
            }
        }
        Dictionary<string, int> nameIndexMap = new Dictionary<string, int>()
    {
        {"PrintInt",0},
    };
        int[] indexIdMap;
        public Action ChooseResponse(int id)
        {
            Action ret;
            if (id == indexIdMap[0])
            {
                ret = new PrintInt();
                ret.id = id;
            }
            else
            {
                throw new System.Exception("未定义的协议id:" + id);
            }
            return ret;
        }
    }
}