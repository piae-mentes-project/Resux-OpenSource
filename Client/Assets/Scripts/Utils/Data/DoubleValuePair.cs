using System.Collections;
using UnityEngine;

namespace Resux
{
    public class DoubleValuePair<T, R>
    {
        public T Value1;
        public R Value2;

        public DoubleValuePair(T t, R r)
        {
            Value1 = t;
            Value2 = r;
        }
    }
}