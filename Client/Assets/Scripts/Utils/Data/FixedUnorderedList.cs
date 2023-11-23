

using System.Collections;
namespace Resux
{
    public class FixedUnorderedList<T> where T : class
    {
        private int Size = 0;
        private int Used = 0;
        private T[] Array;
        public FixedUnorderedList(int _size)
        {
            this.Size = _size;
            this.Array = new T[_size];
        }
        public bool Insert(T obj)
        {
            if (Used >= Size)
            {
                return false;
            }
            for (int i = 0; i < Size; i++)
            {
                if (Array[i] == null)
                {
                    Array[i] = obj;
                    Used++;
                    return true;
                }
            }
            return false;
        }
        public void Remove(T obj)
        {
            for (int i = 0; i < Size; i++)
            {
                if (Array[i] == obj)
                {
                    Array[i] = null;
                    Used--;
                    return;
                }
            }
        }
        public T this[int index] => Array[index];
        public IEnumerator GetEnumerator()
        {
            return Array.GetEnumerator();
        }
    }
}