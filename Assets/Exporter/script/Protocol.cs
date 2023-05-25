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
        {"i32",0}
    };
        int[] indexIdMap;
        public IEnumerable<Data> GetDataToDecode(int id)
        {
            if (id == indexIdMap[0])
            {
                yield return new i32();
            }
            else if (id == indexIdMap[1])
            {
                yield return new f64();
            }
            else if (id == indexIdMap[2])
            {
                yield return new ni32();
            }
            yield break;
        }
    }
}