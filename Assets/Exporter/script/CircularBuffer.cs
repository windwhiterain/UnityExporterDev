using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;
public interface DataSource
{
    public int ReadTo(byte[] buffer, int start, int length);
    public int WriteFrom(byte[] buffer, int start, int length);
}
struct ByteSource : DataSource
{
    byte[] buffer;
    int start, length;
    int index;
    public bool Complete { get { return index == length; } }
    public ByteSource(byte[] buffer, int start, int length)
    {
        this.buffer = buffer;
        this.start = start;
        this.length = length;
        index = 0;
    }
    public int ReadTo(byte[] buffer, int start, int length)
    {
        var left = this.length - this.index;
        var size = Mathf.Min(left, length);
        new Span<byte>(this.buffer, this.start + this.index, size)
        .CopyTo(new Span<byte>(buffer, start, size));
        index += size;
        return size;
    }
    public int WriteFrom(byte[] buffer, int start, int length)
    {
        var left = this.length - this.index;
        var size = Mathf.Min(left, length);
        new Span<byte>(buffer, start, size)
        .CopyTo(new Span<byte>(this.buffer, this.start + this.index, size));
        index += size;
        return size;
    }
}
class CircularBuffer
{

    struct Region
    {
        public (int start, int end)[] startEndArray;
        public int size;
        public Region(params (int start, int end)[] startEndArray)
        {
            this.startEndArray = startEndArray;
            size = 0;
            foreach (var span in startEndArray)
            {
                size += span.end - span.start;
            }
        }
    }
    int size;
    int nextRead;
    int nextWrite;
    byte[] buffer;
    public CircularBuffer(int size)
    {
        buffer = new byte[size];
    }
    Region Writeable()
    {
        if (nextRead > nextWrite)
        {
            return new Region((nextWrite, nextRead));
        }
        else
        {
            return new Region((nextWrite, size), (0, nextRead));
        }
    }
    Region Readable()
    {
        if (nextRead < nextWrite)
        {
            return new Region((nextRead, nextWrite));
        }
        else if (nextRead > nextWrite)
        {
            return new Region((nextRead, size), (0, nextWrite));
        }
        else
        {
            return new Region();
        }
    }
    object writeLock, readLock;
    int NextIndex(int start, int size)
    {
        var ret = start + size;
        if (ret == size) { ret = 0; }
        return ret;
    }
    public void WriteFromSource(DataSource source)
    {
        lock (writeLock)
        {
            var region = Writeable();
            foreach ((int start, int end) in region.startEndArray)
            {
                size = end - start;
                var actualSize = source.ReadTo(buffer, start, size);
                nextWrite = NextIndex(nextWrite, actualSize);
                if (actualSize < size)
                {
                    break;
                }
            }
        }
    }
    public void ReadToSource(DataSource source)
    {
        lock (readLock)
        {
            var region = Readable();
            foreach ((int start, int end) in region.startEndArray)
            {
                size = end - start;
                var actualSize = source.WriteFrom(buffer, start, size);
                nextRead = NextIndex(nextRead, actualSize);
                if (actualSize < size)
                {
                    break;
                }
            }
        }
    }
}


