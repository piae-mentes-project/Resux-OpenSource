using System;
using System.Collections;
using System.Collections.Generic;

namespace Resux
{
    /// <summary>
    /// 优先级队列
    /// </summary>
    /// <typeparam name="T">队列内数据类型</typeparam>
    /// <typeparam name="R">用于做优先级比较的类型</typeparam>
    public class PriorityQueue<T, R> : IEnumerable, IEnumerable<T>, ICollection, IReadOnlyCollection<T>
    where R : IComparable
    {
        List<T> queue;

        Func<T, R> getValueFunc;

        bool isSmallValueFirst;

        public PriorityQueue(Func<T, R> getValueFunc, bool isSmallValueFirst = true)
        {
            queue = new List<T>();
            this.getValueFunc = getValueFunc;
            this.isSmallValueFirst = isSmallValueFirst;
        }

        #region Public Methods

        public void Clear()
        {
            queue.Clear();
        }

        public bool Contains(T item)
        {
            return queue.Contains(item);
        }

        public T Dequeue()
        {
            if (Count > 0)
            {
                var item = queue[0];
                queue.RemoveAt(0);
                return item;
            }

            return default;
        }

        public void Enqueue(T item)
        {
            if (Count == 0)
            {
                queue.Add(item);
                return;
            }

            // 比较优先级
            var val = getValueFunc(item);
            var queueCount = Count;
            for (int i = 0; i < queueCount; i++)
            {
                // 队头值小于插入值
                if (getValueFunc(queue[i]).CompareTo(val) < 0)
                {
                    if (isSmallValueFirst)
                    {
                        queue.Add(item);
                    }
                    else
                    {
                        queue.Insert(i, item);
                    }
                }
                else
                {
                    if (isSmallValueFirst)
                    {
                        queue.Insert(i, item);
                    }
                    else
                    {
                        queue.Add(item);
                    }
                }
            }
        }

        public T Peek()
        {
            if (Count > 0)
            {
                return queue[0];
            }

            return default;
        }

        public T[] ToArray()
        {
            return queue.ToArray();
        }

        public void TrimExcess()
        {
            queue.TrimExcess();
        }

        public bool TryDequeue(out T result)
        {
            if (Count > 0)
            {
                var item = Dequeue();
                result = item;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryPeek(out T result)
        {
            if (Count > 0)
            {
                var item = Peek();
                result = item;
                return true;
            }

            result = default;
            return false;
        }

        #endregion

        #region implement

        public int Count => queue.Count;

        bool ICollection.IsSynchronized => ((ICollection)queue).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)queue).SyncRoot;

        public void CopyTo(Array array, int index)
        {
            ((ICollection)queue).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)queue).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return ((IEnumerable<T>)queue).GetEnumerator();
        }

        #endregion
    }

}