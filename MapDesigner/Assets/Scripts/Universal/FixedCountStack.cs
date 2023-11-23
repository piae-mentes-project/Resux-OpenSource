using System;
using System.Collections.Generic;

/// <summary>
/// 固定元素个数的栈
/// </summary>
/// <typeparam name="T">元素类型</typeparam>
public class FixedCountStack<T>
{
    #region properties

    public readonly int TotalCount;

    private T[] elements;

    private int currentCount;
    public int Count => currentCount;

    private int startIndex;

    private int StartIndex
    {
        get => startIndex;
        set
        {
            startIndex = value;
            startIndex %= TotalCount;
        }
    }

    #endregion

    public FixedCountStack(int count)
    {
        TotalCount = count;
        elements = new T[TotalCount];
        currentCount = 0;
        startIndex = 0;
    }

    #region Public Method

    public T Peek()
    {
        if (currentCount <= 0)
        {
            return default(T);
        }

        return elements[GetTopIndex()];
    }

    public T Pop()
    {
        if (currentCount <= 0)
        {
            return default(T);
        }

        var topIndex = GetTopIndex();

        var element = elements[topIndex];
        elements[topIndex] = default(T);
        currentCount--;

        return element;
    }

    public void Push(T element)
    {
        var nextIndex = GetNextIndex();
        elements[nextIndex] = element;

        if (currentCount >= TotalCount)
        {
            StartIndex++;
        }
        else
        {
            currentCount++;
        }
    }

    public void Clear()
    {
        startIndex = currentCount = 0;
        for (int i = 0; i < Count; i++)
        {
            elements[i] = default(T);
        }
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 获取顶部元素的索引
    /// </summary>
    private int GetTopIndex()
    {
        return (StartIndex + currentCount - 1) % TotalCount;
    }

    /// <summary>
    /// 获取下一个元素的索引
    /// </summary>
    private int GetNextIndex()
    {
        return (StartIndex + currentCount) % TotalCount;
    }

    #endregion
}
