using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Exporter
{
    using i32 = SmallData<System.Int32>;
    using f64 = SmallData<System.Single>;
    using ni32 = ArrayData<System.Int32>;
    public abstract class Action
    {
        public int id;
        public bool Ready => InputData.Complete;
        public abstract Data InputData { get; }
        public abstract void Excecute();
    }
    public class PrintInt : Action
    {
        i32 toPrint;
        public PrintInt(int value)
        {
            toPrint = new i32(value);
        }
        public override Data InputData => toPrint;
        public override void Excecute()
        {
            Debug.Log(toPrint.Uncoded);
        }
    }
}
