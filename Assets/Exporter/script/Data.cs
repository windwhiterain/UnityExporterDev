using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exporter
{

    public abstract class Data
    {
        public abstract void Read(DataSource source);
        public abstract void Write(DataSource source);
        protected bool complete = false;
        public bool Complete { get { return complete; } }
        protected int CodedLength<T>(T obj) { throw new System.Exception(); }
        protected int CodedLength(System.Int32 obj) => sizeof(System.Int32);
        protected int CodedLength(float obj) => sizeof(float);
        protected int CodedLength<T>()
        {
            T temp = default;
            return CodedLength(temp);
        }
        protected void EnCode<T>(T obj, System.Span<byte> coded) { throw new System.Exception(); }
        protected void EnCode(System.Int32 obj, System.Span<byte> coded)
        {
            System.BitConverter.TryWriteBytes(coded, obj);
        }
        protected void EnCode(float obj, System.Span<byte> coded)
        {
            System.BitConverter.TryWriteBytes(coded, obj);
        }
        protected T DeCode<T>(System.Span<byte> coded, T obj) { throw new System.Exception(); }
        protected System.Int32 DeCode(System.Span<byte> coded, System.Int32 obj)
        {
            return System.BitConverter.ToInt32(coded);
        }
        protected float DeCode(System.Span<byte> coded, float obj)
        {
            return System.BitConverter.ToSingle(coded);
        }
        protected T DeCode<T>(System.Span<byte> coded)
        {
            T temp = default;
            return DeCode(coded, temp);
        }
    }
    public abstract class SmallData<T> : Data
    {
        protected byte[] coded;
        int completeIndex = 0;
        public SmallData(T uncoded)
        {
            coded = new byte[CodedLength(uncoded)];
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
    public class i32 : SmallData<System.Int32> { }
    public class f64 : SmallData<float> { }
    public class ArrayData<T> : Data
    {
        byte[] coded;
        i32 countData;
        int count;
        int completeIndex;
        public ArrayData(T[] uncoded)
        {
            count = uncoded.Length;
            var cell = CodedLength<T>();
            var size = uncoded.Length * cell;
            coded = new byte[size + 1];
            var span = new System.Span<byte>(coded, 0, cell);
            EnCode(uncoded.Length, span);
            for (int index = 1; index < uncoded.Length + 1; index++)
            {
                var _span = new System.Span<byte>(coded, cell * index, cell);
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
                var span = new System.Span<byte>(coded, 0, cell);
                var Length = DeCode<System.Int32>(span);
                var uncoded = new T[Length];
                for (int index = 1; index < Length + 1; index++)
                {
                    var _span = new System.Span<byte>(coded, cell * index, cell);
                    uncoded[index] = DeCode<T>(span);
                }
                return uncoded;
            }
        }
    }
    public class ni32 : ArrayData<System.Int32> { }
}