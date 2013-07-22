using System;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using System.Linq;
using System.Text;

namespace myQv.Core
{
    public class Tuple<T>
    {
        public Tuple(T first)
        {
            First = first;
        }

        public T First { get; set; }
    }

    public class Tuple<T1, T2> : Tuple<T1>
    {
        public Tuple(T1 first, T2 second)
            : base(first)
        {
            Second = second;
        }

        public T2 Second { get; set; }
    }

    public class Tuple<T1, T2, T3> : Tuple<T1, T2>
    {
        public Tuple(T1 first, T2 second, T3 third)
            : base(first, second)
        {
            Third = third;
        }

        public T3 Third { get; set; }
    }

    public class Tuple<T1, T2, T3, T4> : Tuple<T1, T2, T3>
    {
        public Tuple(T1 first, T2 second, T3 third, T4 fourth)
            : base(first, second, third)
        {
            Fourth = fourth;
        }

        public T4 Fourth { get; set; }
    }

    public static class Tuple
    {
        public static Tuple<T> New<T>(T t)
        {
            return new Tuple<T>(t);
        }

        public static Tuple<T1, T2> New<T1, T2>(T1 t1, T2 t2)
        {
            return new Tuple<T1, T2>(t1, t2);
        }

        public static Tuple<T1, T2, T3> New<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
        {
            return new Tuple<T1, T2, T3>(t1, t2, t3);
        }

        public static Tuple<T1, T2, T3, T4> New<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            return new Tuple<T1, T2, T3, T4>(t1, t2, t3, t4);
        }

    }



    public class Nictionary<T1, T2> : Dictionary<T1, T2>
    {
    }

    public class Nictionary<T1, T2, T3> : Dictionary<T1, Tuple<T2, T3>>
    {
        public void Add(T1 t1, T2 t2, T3 t3)
        {
            base.Add(t1, Tuple.New(t2, t3));
        }
    }

    public class Nictionary<T1, T2, T3, T4> : Dictionary<T1, Tuple<T2, T3, T4>>
    {
        public void Add(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            base.Add(t1, Tuple.New(t2, t3, t4));
        }
    }

    public class Nictionary<T1, T2, T3, T4, T5> : Dictionary<T1, Tuple<T2, T3, T4, T5>>
    {
        public void Add(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            base.Add(t1, Tuple.New(t2, t3, t4, t5));
        }
    }

}
