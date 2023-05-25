using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exporter
{
    using i32 = SmallData<System.Int32>;
    using f64 = SmallData<System.Single>;
    using ni32 = ArrayData<System.Int32>;
    public abstract class Data
    {
        public abstract void Read(DataSource source);
        public abstract void Write(DataSource source);
        protected bool complete = false;
        public bool Complete { get { return complete; } }
        protected int CodedLength<T>()
        {
            T temp = default;
            if (temp is System.Int32)
            {
                return sizeof(System.Int32);
            }
            else if (temp is System.Single)
            {
                return sizeof(System.Single);
            }
            else
            {
                throw new System.Exception();
            }
        }
        protected void EnCode<T>(T obj, System.Span<byte> coded)
        {
            if (obj is System.Int32 int32)
            {
                System.BitConverter.TryWriteBytes(coded, int32);
            }
            else if (obj is System.Single single)
            {
                System.BitConverter.TryWriteBytes(coded, single);
            }
            else
            {
                throw new System.Exception();
            }
        }
        protected T DeCode<T>(System.Span<byte> coded)
        {
            T temp = default;
            if (temp is System.Int32)
            {
                var _ret = System.BitConverter.ToInt32(coded);
                if (_ret is T ret)
                {
                    return ret;
                }
            }
            if (temp is System.Single)
            {
                var _ret = System.BitConverter.ToSingle(coded);
                if (_ret is T ret)
                {
                    return ret;
                }
            }
            throw new System.Exception();
        }
    }
    public class SmallData<T> : Data
    {
        protected byte[] coded;
        int completeIndex = 0;
        public SmallData(T uncoded)
        {
            coded = new byte[CodedLength<T>()];
            EnCode(uncoded, coded);
        }
        public SmallData()
        {
            coded = new byte[CodedLength<T>()];
        }
        public override void Read(DataSource source)
        {
            int left = coded.Length - completeIndex;
            completeIndex += source.Write(coded, completeIndex, left);
            if (completeIndex == coded.Length) { complete = true; }
        }
        public override void Write(DataSource source)
        {
            int left = coded.Length - completeIndex;
            completeIndex += source.Read(coded, completeIndex, left);
            if (completeIndex == coded.Length) { complete = true; }
        }
        public T Uncoded => DeCode<T>(coded);
    }
    public class ArrayData<T> : Data
    {
        byte[] coded;
        int count;
        i32 countData;
        int completeIndex;
        public ArrayData(T[] uncoded)
        {
            var cell = CodedLength<T>();
            var i32Cell = CodedLength<System.Int32>();
            var size = uncoded.Length * cell + i32Cell;
            coded = new byte[size];
            var span = new System.Span<byte>(coded, 0, i32Cell);
            EnCode(uncoded.Length, span);
            for (int index = 0; index < uncoded.Length; index++)
            {
                var _span = new System.Span<byte>(coded, i32Cell + cell * index, cell);
                EnCode(uncoded[index], _span);
            }
        }
        public ArrayData()
        {
            countData = new i32();
        }
        public override void Read(DataSource source)
        {
            if (!countData.Complete)
            {
                countData.Read(source);
                if (!countData.Complete) { return; }
                else
                {
                    count = countData.Uncoded;
                    coded = new byte[CodedLength<T>() * count];
                }
            }
            int left = coded.Length - completeIndex;
            completeIndex += source.Write(coded, completeIndex, left);
            if (completeIndex == coded.Length) { complete = true; }
        }
        public override void Write(DataSource source)
        {
            int left = coded.Length - completeIndex;
            completeIndex += source.Read(coded, completeIndex, left);
            if (completeIndex == coded.Length) { complete = true; }
        }
        public T[] Uncoded
        {
            get
            {
                var cell = CodedLength<T>();
                var uncoded = new T[count];
                for (int index = 0; index < count; index++)
                {
                    var span = new System.Span<byte>(coded, cell * index, cell);
                    uncoded[index] = DeCode<T>(span);
                }
                return uncoded;
            }
        }
    }
}