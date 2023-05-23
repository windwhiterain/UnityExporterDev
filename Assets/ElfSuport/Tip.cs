using System.ComponentModel;
using System.Net.WebSockets;
using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Tip
{
#if UNITY_EDITOR
    /// <summary>
    /// is the first level subPrefab of a Prefab 
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static bool isFirstClassInnerPrefabInstance(GameObject gameObject)
    {
        var trans = gameObject.transform.parent;
        if (trans == null)
        {
            if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                return true;
            }
            return false;
        }
        while (true)
        {
            if (trans.parent == null)
            {
                return true;
            }
            else if (UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(trans.gameObject))
            {
                return false;
            }
            trans = trans.parent;
        }
    }
    /// <summary>
    /// is the first level subPrefab of a Prefab A in the prefabScene of A 
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static bool isFirstClassInnerPrefabInstanceInScene(GameObject gameObject)
    {
        if (!UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(gameObject)) { return false; }
        if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) { return false; }
        return isFirstClassInnerPrefabInstance(gameObject);
    }
    /// <summary>
    /// is the first level subPrefab of a Prefab A in the asset of A 
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public static bool isFirstClassInnerPrefabInstanceInAsset(GameObject gameObject)
    {
        if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)) { return false; }
        return isFirstClassInnerPrefabInstance(gameObject);
    }
#endif
    /// <summary>
    /// a list that won't change in malloc size
    /// </summary>
    public class StaticList<T> : IEnumerable<T>
    {
        public T[] arr;
        /// <summary>
        /// the size malloced 
        /// </summary>
        public int maxCount;
        /// <summary>
        /// </summary>
        /// <param name="maxCount">the size to malloc</param>
        public StaticList(int maxCount)
        {
            arr = new T[maxCount];
            this.maxCount = maxCount;
        }
        public StaticList(T[] arr)
        {
            this.arr = arr;
            this.maxCount = arr.Length;
            Count = arr.Length;
        }
        public StaticList(List<T> list)
        {
            this.arr = list.ToArray();
            this.maxCount = list.Count;
            Count = list.Count;
        }
        public StaticList(IEnumerable<T> iterator, int count)
        {
            arr = new T[count];
            int index = 0;
            foreach (var item in iterator)
            {
                arr[index] = item;
            }
            maxCount = count;
            Count = count;
        }
        public StaticList(IEnumerable<T> iterator)
        {
            List<T> temp = new List<T>();
            foreach (var item in iterator)
            {
                temp.Add(item);
            }
            arr = temp.ToArray();
            maxCount = temp.Count;
            Count = temp.Count;
        }
        /// <summary>
        /// the count of added item
        /// </summary>
        public int Count = 0;
        /// <summary>
        /// add an item into it but should not make the items more than max Count
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            arr[Count] = item;
            Count++;
        }
        public void Clear()
        {
            Count = 0;
        }
        public ref T this[int index]
        {
            get
            {
                return ref arr[index];
            }
        }
        IEnumerator<T> IE()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return arr[i];
            }
        }
        IEnumerator _IE()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return arr[i];
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return IE();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _IE();
        }
        static public implicit operator T[](StaticList<T> self)
        {
            return self.arr;
        }
        public override string ToString()
        {
            return Tip.Show(this);
        }
        public StaticList<T> this[Range range]
        {
            get
            {
                return new StaticList<T>(arr[range]);
            }
        }
    }
    /// <summary>
    /// generate the debug message of a List<>
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>every item's ToString()</returns>
    public static string Show<T>(this IEnumerable<T> list)
    {
        string ret = "[";
        foreach (var item in list)
        {
            ret += item.ToString() + ",";
        }
        return ret + "]";
    }
    /// <summary>
    /// add an item to it by malloc a new longer array to relace it
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] Add<T>(this T[] arr, T item)
    {
        if (arr == null)
        {
            arr = new T[] { item };
            return arr;
        }
        var ret = new T[arr.Length + 1];
        ret[arr.Length] = item;
        arr.CopyTo(ret, 0);
        arr = ret;
        return ret;
    }
    /// <summary>
    /// to make a degree range from 0~360 to -180~180
    /// </summary>
    /// <param name="x">degree</param>
    /// <returns></returns>
    public static float Halflize(this float x)
    {
        if (x > 180) { return x - 360; }
        else { return x; }
    }
    /// <summary>
    /// to make a degree range from -180~18 to 0~360 
    /// </summary>
    /// <param name="x">degree</param>
    /// <returns></returns>
    public static float Fulllize(this float x)
    {
        if (x < 0) { return x + 360; }
        else { return x; }
    }
    /// <summary>
    /// to make a degree v3 range from 0~360 to -180~180
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 Halflize(this Vector3 v)
    {
        return new Vector3(v.x.Halflize(), v.y.Halflize(), v.z.Halflize());
    }
    /// <summary>
    /// to make a degree v3 range from -180~18 to 0~360 
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 Fulllize(this Vector3 v)
    {
        return new Vector3(v.x.Fulllize(), v.y.Fulllize(), v.z.Fulllize());
    }
    /// <summary>
    /// Get All first level children of a transform
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IEnumerable<Transform> GetChildren(this Transform transform)
    {
        var children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        foreach (var child in children)
        {
            yield return child;
        }
    }
    /// <summary>
    /// set everything to be the same as the target
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    public static void ChangeTo(this Transform origin, Transform target)
    {
        origin.parent = target.parent;
        origin.localPosition = target.localPosition;
        origin.rotation = target.rotation;
        origin.localScale = target.localScale;
    }
    /// <summary>
    /// get the highest 1 bit's (index+1) in this binary form,return 0 means no 1 bit 
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int HighestBit(this int a)
    {
        if (a == 0) { return 0; }
        int move = 16;
        for (int div = move >> 1; div > 0; div >>= 1)
        {
            if ((a >> move) == 0) { move -= div; }
            else { move += div; }
        }
        if ((a >> move) == 0) { return move; }
        else { return move + 1; }
    }
    /// <summary>
    /// get the index of min second power of this
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int MinGreaterSecondPower(this int a)
    {
        int len = a.HighestBit() - 1;
        if ((a >> len << len) == a) { return len; }
        else { return len + 1; }
    }
    public static int MinGreaterSecondPowerNumber(this int a)
    {
        return 1 << a.MinGreaterSecondPower();
    }
    public static int PowerToValue(this int a)
    {
        return 1 << a;
    }
    public static void Init<T>(out T item) where T : unmanaged
    {
        unsafe
        {
            fixed (T* ptr = &item)
            {
                var bptr = (byte*)ptr;
                for (int i = 0; i < sizeof(T); i++)
                {
                    bptr[i] = 0;
                }
            }
        }
    }
    public static Vector3 V3(Vector2 v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }
    public static Vector2 V2(Vector2Int v2)
    {
        return new Vector2(v2.x, v2.y);
    }
    public static Vector3 V3(Vector3Int v3)
    {
        return new Vector3(v3.x, v3.y, v3.z);
    }
    public static Vector3 V3(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }
    public static Vector2 Rot(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x - a.y * b.y, a.y * b.x + a.x * b.y);
    }
    public static Vector2 zx(this Vector3 self)
    {
        return new Vector2(self.z, self.x);
    }
    public static Vector3 GeometryAverage(Vector3 a, Vector3 b)
    {
        if (a == Vector3.zero && b == Vector3.zero) { return Vector3.zero; }
        float m1 = a.magnitude, m2 = b.magnitude;
        return (a * m2 + b * m1) / (m1 + m2);
    }
    public static float Average(IEnumerable<float> elements)
    {
        float sum = 0f;
        int count = 0;
        foreach (var item in elements)
        {
            sum += item;
            count++;
        }
        return sum / count;
    }
    [Serializable]
    public class AverageCounter
    {
        public float value;
        Queue<float> values;
        public int Count;
        public void Add(float value)
        {
            if (values.Count >= Count) { values.Dequeue(); }
            values.Enqueue(value);
            value = Average(values);
        }
        public AverageCounter(int Count)
        {
            this.Count = Count;
            values = new Queue<float>(Count);
            value = 0;
        }
    }
    public static IEnumerable<System.Reflection.FieldInfo> GetFieldsByAttributeType<AttributeType>(this object self) where AttributeType : System.Attribute
    {
        var t = self.GetType();
        foreach (var field in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            var req = System.Attribute.GetCustomAttribute(field, typeof(AttributeType), true);
            if (req != null)
            {
                yield return field;
            }
        }
    }
    public static IEnumerable<Transform> GetAllLevelChildren(this Transform trans)
    {
        foreach (var child in trans.GetChildren())
        {
            foreach (var child2 in GetAllLevelChildren(child))
            {
                yield return child2;
            }
        }
        yield return trans;
    }
    public static IEnumerable<ComponentType> GetComponentsInAllLevel<ComponentType>(this Transform transform) where ComponentType : UnityEngine.Component
    {
        foreach (var trans in transform.GetAllLevelChildren())
        {
            foreach (var comp in trans.GetComponents<ComponentType>())
            {
                yield return comp;
            }
        }
    }
    public static IEnumerable<Transform> GetAllUpper(this Transform transform)
    {
        while (true)
        {
            yield return transform;
            if (transform.parent == null) { yield break; }
            else { transform = transform.parent; }
        }
    }
    public static void Copy<T>(T[] source, int sourceStart, T[] dest, int destStart, int count)
    {
        for (int i = 0; i < count; i++)
        {
            dest[destStart + i] = source[sourceStart + i];
        }
    }
    public static void CreateRWNoDepth(out RenderTexture target, int width, int height
    , UnityEngine.Experimental.Rendering.GraphicsFormat format)
    {
        target = new RenderTexture(width, height, 0, format);
        target.enableRandomWrite = true;
        target.Create();
    }
    public static void CreateRWNoDepth(out RenderTexture target, Texture2D size
    , UnityEngine.Experimental.Rendering.GraphicsFormat format)
    {
        CreateRWNoDepth(out target, size.width, size.height, format);
    }
    public static void CreateRWNoDepth(out RenderTexture target, RenderTexture model)
    {
        target = new RenderTexture(model.width, model.height, 0, model.format);
        target.enableRandomWrite = true;
        target.Create();
    }
    public struct Slice
    {
        public int start;
        public int count;
        public int end { get { return start + count; } }
        public Slice(int start, int count)
        {
            this.start = start;
            this.count = count;
        }
        public override string ToString()
        {
            return "{" + start + "," + count + "}";
        }
    }
}
