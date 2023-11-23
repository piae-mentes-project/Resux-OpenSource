using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : class
{
    #region properties

    private Queue<T> objects;
    /// <summary>��������ķ���</summary>
    private Func<T> createObjectFunc;
    /// <summary>���ն���ʱ�Ĳ���</summary>
    private Action<T> returnObjectAction;

    #endregion

    public ObjectPool(int num, Func<T> createFunc, Action<T> returnAction)
    {
        objects = new Queue<T>(num);
        createObjectFunc += createFunc;
        returnObjectAction += returnAction;
        for (int i = 0; i < num; i++)
        {
            objects.Enqueue(createObjectFunc());
        }
    }

    #region Public Method

    public T GetObject()
    {
        if (objects.Count > 0)
        {
            return objects.Dequeue();
        }

        return createObjectFunc();
    }

    public void ReturnToPool(T @object)
    {
        returnObjectAction?.Invoke(@object);
        objects.Enqueue(@object);
    }

    #endregion

    #region Private Method



    #endregion
}
