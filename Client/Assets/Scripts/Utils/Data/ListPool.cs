using System.Collections;
using System.Collections.Generic;

/// <summary>
///
/// name:ListPool
/// author:Administrator
/// date:2017/2/21 16:55:18
/// versions:
/// introduce:
/// note:
/// 
/// </summary>
namespace Resux
{
    internal static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> _listPool = new ObjectPool<List<T>>(5, () => new List<T>(), l => l.Clear());

        public static List<T> Get()
        {
            return _listPool.GetObject();
        }

        public static void Recycle(List<T> element)
        {
            _listPool.ReturnToPool(element);
        }
    }
}
