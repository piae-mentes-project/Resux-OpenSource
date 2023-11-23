using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resux
{
    /// <summary>
    /// 无序容器，成员只减不增。
    /// 如果没有变动过，直接foreach obj.Array即可，
    /// ！ReduceUnorderedList元素可能 **有过** 变动的时候请使用for循环！
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReduceUnorderedList<T> where T : class
    {
        private int _Size = 0;
        private int Total = 0;
        public int Length => _Size;
        public readonly T[] Array = null;
        private int Offset = 0;
        private bool EnableSwap = false;

        public ReduceUnorderedList(T[] arr, bool swap = false)
        {
            Array = arr;
            Total = _Size = arr.Length;
            EnableSwap = swap;
        }

        public T this[int index] => Array[Offset + index];

        public void Remove(T obj)
        {
            for (int i = Offset; i < Total; i++)
            {
                if (Array[i] == obj)
                {
                    if (EnableSwap)
                    {
                        (Array[Offset], Array[i]) = (Array[i], Array[Offset]);
                    }
                    else
                    {
                        Array[i] = Array[Offset];
                        Array[Offset] = null;
                    }

                    Offset++;
                    _Size--;
                    return;
                }
            }
        }
    }
}
